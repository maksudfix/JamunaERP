namespace JamunaERP.Application.Contracts;

public interface ICurrentUserService
{
    // Identity user ID of the caller, or null outside an HTTP request.
    string? UserId { get; }

    // Returns false when there is no current HTTP user.
    bool IsInRole(string role);
}