using JamunaERP.Domain.Enums;
using MediatR;

namespace JamunaERP.Application.StockItems.Commands.CreateStockItem
{
    public record CreateStockItemCommand(
        int ProductId,
        StockLocationType Location,
        int Quantity,
        bool IsFaulty,
        string? SerialNumber
    ) : IRequest<int>;
}