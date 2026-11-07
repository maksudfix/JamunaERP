using System.Security.Claims;
using JamunaERP.Application.Common.Security;
using JamunaERP.Application.Contracts;
using JamunaERP.Application.Features.Authentication.Commands.Register;
using JamunaERP.Application.Login.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace JamunaERP.WebApi.Controllers;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(ISender mediator, IAuthService authService, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _authService = authService;
        _currentUser = currentUser;
    }

    // Only an Admin can create new users and assign their role
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result)
            return BadRequest(new { message = "Registration failed. Email might already be taken or password requirements not met." });

        return Ok(new { message = "User registered successfully!" });
    }

    // Public: this is how users get their token
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var token = await _mediator.Send(command);
        if (token == null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(new { token });
    }

    // Authenticated: issues a fresh JWT token if the user is active and within MaxSessionHours
    [Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        long? authTime = long.TryParse(User.FindFirstValue("auth_time"), out var parsed) ? parsed : null;

        var token = await _authService.RefreshTokenAsync(userId, authTime);
        if (token == null)
            return Unauthorized(new { message = "Session expired or user account is no longer active. Please log in again." });

        return Ok(new { token });
    }

    // Authenticated: allows any logged-in user to change their own password
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "Current password and new password are required." });

        var changed = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (!changed)
            return BadRequest(new { message = "Password change failed. Check your current password and ensure the new password meets complexity rules." });

        return Ok(new { message = "Password changed successfully. Please log in with your new password." });
    }
}