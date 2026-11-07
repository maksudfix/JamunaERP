using MediatR;

namespace JamunaERP.Application.Customers.Commands.CreateCustomer
{
    public record CreateCustomerCommand(
        string Name,
        string Phone,
        string Address,
        string? NidNumber = null,
        string? AlternatePhone = null
    ) : IRequest<int>;
}