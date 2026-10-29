using System.Security.Claims;
using JamunaERP.Application.Contracts;

namespace JamunaERP.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public bool IsInRole(string role) =>
        _accessor.HttpContext?.User?.IsInRole(role) ?? false;

    public string? UserId
    {
        get
        {
            var user = _accessor.HttpContext?.User;

            return user?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? user?.FindFirstValue("sub");
        }
    }
}