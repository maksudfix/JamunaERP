using MediatR;

namespace JamunaERP.Application.Login.Commands;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<string?>; // Returns JWT token string on success