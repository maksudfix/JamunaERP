using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JamunaERP.Application.Contracts;
using JamunaERP.Domain.Enums;
using JamunaERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JamunaERP.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IAuthService
{
    public async Task<bool> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        StockLocationType assignedLocation = StockLocationType.Showroom1)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            AssignedLocation = assignedLocation
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) return false;

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return false;
        }

        return true;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive) return null;

        if (await userManager.IsLockedOutAsync(user)) return null;

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        var authTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return GenerateJwtToken(user, roles, authTimeUnix);
    }

    public async Task<string?> RefreshTokenAsync(string userId, long? originalAuthTimeUnix = null)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive || await userManager.IsLockedOutAsync(user))
            return null;

        var maxSessionHours = int.TryParse(configuration["JwtSettings:MaxSessionHours"], out var h) && h > 0
            ? h
            : 12;

        var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var authTime = originalAuthTimeUnix ?? nowUnix;
        var elapsedSeconds = nowUnix - authTime;

        if (elapsedSeconds > TimeSpan.FromHours(maxSessionHours).TotalSeconds)
            return null; // Maximum session duration exceeded; user must log in again

        var roles = await userManager.GetRolesAsync(user);
        return GenerateJwtToken(user, roles, authTime);
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
            return false;

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            return false;

        await userManager.UpdateSecurityStampAsync(user);
        return true;
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles, long authTimeUnix)
    {
        var jwt = configuration.GetSection("JwtSettings");
        var secret = jwt["Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");
        var expiryMinutes = int.TryParse(jwt["ExpiryMinutes"], out var m) ? m : 60;

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.GivenName, user.FirstName ?? ""),
            new(ClaimTypes.Surname, user.LastName ?? ""),
            new("AssignedLocation", user.AssignedLocation.ToString()),
            new("SecurityStamp", user.SecurityStamp ?? ""),
            new("auth_time", authTimeUnix.ToString(CultureInfo.InvariantCulture))
        ];
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}