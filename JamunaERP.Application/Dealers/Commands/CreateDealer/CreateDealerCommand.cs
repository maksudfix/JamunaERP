using MediatR;

namespace JamunaERP.Application.Dealers.Commands.CreateDealer
{
    public record CreateDealerCommand(
        string Name,
        string ContactPerson,
        string Phone,
        string Address
    ) : IRequest<int>;
}