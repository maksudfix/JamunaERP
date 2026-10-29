using MediatR;
using System;

namespace JamunaERP.Application.PurchaseOrders.Commands.CreatePurchaseOrder
{
    public record CreatePurchaseOrderCommand(
        string DealerInvoiceNo,
        int DealerId,
        DateTime ArrivalDate,
        DateTime? NextLotEstimatedDate
    ) : IRequest<int>;
}