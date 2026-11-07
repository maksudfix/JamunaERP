using JamunaERP.Application.Common.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JamunaERP.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var admin = await userManager.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Could not seed admin: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
            await userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}