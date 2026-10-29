using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Contracts;
using JamunaERP.Infrastructure.Data;
using JamunaERP.Infrastructure.Identity;
using JamunaERP.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JamunaERP.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Register EF Core Database Context with SQL Server, Migrations Assembly and audit interceptor
            services.AddScoped<AuditSaveChangesInterceptor>();

            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
                options
                    .UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                    .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

            // 2. Map IAppDbContext to concrete AppDbContext so Application layer can use it
            services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

            // 3. Register ASP.NET Core Identity Core for Class Library
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

            // 4. Register Infrastructure Services (e.g., AuthService)
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}