using JamunaERP.Application;
using JamunaERP.Application.Contracts;
using JamunaERP.Infrastructure;
using JamunaERP.Infrastructure.Data;
using JamunaERP.Infrastructure.Identity;
using JamunaERP.WebApi.HealthChecks;
using JamunaERP.WebApi.Middleware;
using JamunaERP.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

// Simple logger that works even before the app is fully built,
// so startup errors are always shown in the console.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Forwarded Headers (for running behind a proxy like Nginx or IIS)
    builder.Services.Configure<ForwardedHeadersOptions>(o =>
    {
        o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

    // Serilog (reads the "Serilog" section from appsettings.json)
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddControllers()
        .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    // 1. Application layer (MediatR & FluentValidation)
    builder.Services.AddApplicationServices(builder.Configuration["MediatR:LicenseKey"]);

    // 2. Current user (needed by the audit interceptor in Infrastructure)
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // 3. Infrastructure layer (DbContext, Identity, AuthService)
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // 4. JWT authentication
    var jwt = builder.Configuration.GetSection("JwtSettings");
    var secret = jwt["Secret"]
        ?? throw new InvalidOperationException(
            "JwtSettings:Secret is not configured. Add it to user-secrets or an environment variable.");

    // Refuse to start with a weak signing key
    if (secret.Length < 32)
        throw new InvalidOperationException("JwtSettings:Secret must be at least 32 characters.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwt["Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                NameClaimType = System.Security.Claims.ClaimTypes.Name,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };
        });

    // Every endpoint requires a logged-in user unless it is marked [AllowAnonymous]
    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    // 5. CORS: only the frontend addresses listed in "Cors:AllowedOrigins"
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                         ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
        });
    });

    // 6. Rate limiting (per client IP)
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        static string ClientKey(HttpContext ctx) =>
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 600,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

        options.AddPolicy("login", ctx =>
            RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    });

    // 7. Health check
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database");

    // 8. OpenAPI with a Bearer scheme so Scalar can send JWT tokens
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };
            return Task.CompletedTask;
        });
    });

    var app = builder.Build();

    app.UseForwardedHeaders();

    // NEW: turns exceptions into proper 400 / 404 / 409 / 422 answers instead of plain 500 errors
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // NEW: only redirect http -> https outside Development (so local testing stays simple)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    // Optional: apply migrations on startup if configured
    using (var scope = app.Services.CreateScope())
    {
        if (app.Configuration.GetValue("Database:AutoMigrate", false))
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }
    }

    // NEW: create the Admin / Manager / Employee roles and the first admin user.
    // The admin is only created if SeedAdmin:Email and SeedAdmin:Password exist in user-secrets.
    using (var seedScope = app.Services.CreateScope())
    {
        await IdentitySeeder.SeedAsync(seedScope.ServiceProvider, app.Configuration);
    }

    // --- REQUEST PIPELINE MIDDLEWARE (REQUIRED) ---
    app.UseCors("Frontend");
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health").AllowAnonymous();

    // Map OpenAPI & Scalar documentation in Development
    // AllowAnonymous = you can open these pages without logging in
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("JamunaERP API");
        }).AllowAnonymous();

        // Automatically open the browser on the Scalar page when the app starts
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://localhost:7215/scalar/v1",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not open the browser automatically");
            }
        });
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // Shows the real reason the app failed to start
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("=== APP FAILED TO START ===");
    Console.WriteLine(ex.Message);
    Console.ResetColor();
    Log.Fatal(ex, "Application terminated unexpectedly");
    Console.WriteLine("Press any key to close...");
    Console.ReadKey();
}
finally
{
    Log.CloseAndFlush();
}