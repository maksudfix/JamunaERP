namespace JamunaERP.Application.Contracts;

public interface ICurrentUserService
{
    // Identity user ID of the caller, or null outside an HTTP request (e.g. seeding, tests)
    string? UserId { get; }
}