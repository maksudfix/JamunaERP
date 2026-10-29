using JamunaERP.Application.Common.Security;
using JamunaERP.Application.Features.Authentication.Commands.Register;
using JamunaERP.Application.Login.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace JamunaERP.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
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
}