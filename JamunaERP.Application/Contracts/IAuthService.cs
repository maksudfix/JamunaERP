namespace JamunaERP.Application.Contracts;

public interface IAuthService
{
    Task<bool> RegisterAsync(string email, string password, string firstName, string lastName, string role);
    Task<string?> LoginAsync(string email, string password); // Returns token or null
}