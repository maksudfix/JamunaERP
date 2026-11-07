using MediatR;

namespace JamunaERP.Application.PurchaseOrderItems.Commands.CreatePurchaseOrderItem
{
    public record CreatePurchaseOrderItemCommand(
        int PurchaseOrderId,
        int ProductId,
        int QuantityArrived,
        int FaultyQuantityReceived,
        decimal UnitCost
    ) : IRequest<int>;
}