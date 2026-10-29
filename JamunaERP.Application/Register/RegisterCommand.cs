using MediatR;

namespace JamunaERP.Application.Features.Authentication.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role
) : IRequest<bool>;