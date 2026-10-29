using MediatR;

namespace JamunaERP.Application.Customers.Commands.CreateCustomer
{
    public record CreateCustomerCommand(
        string Name,
        string Phone,
        string Address
    ) : IRequest<int>;
}