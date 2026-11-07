using JamunaERP.Domain.Enums;

namespace JamunaERP.Application.Contracts;

public interface IAuthService
{
    Task<bool> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        StockLocationType assignedLocation = StockLocationType.Showroom1);

    Task<string?> LoginAsync(string email, string password);

    Task<string?> RefreshTokenAsync(string userId, long? originalAuthTimeUnix = null);

    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}