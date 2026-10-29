using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.PurchaseOrders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.PurchaseOrderItems.Queries.GetPurchaseOrderItemsByOrder;

public record GetPurchaseOrderItemsByOrderQuery(int PurchaseOrderId) : IRequest<List<PurchaseOrderItemDto>>;

public class GetPurchaseOrderItemsByOrderQueryHandler : IRequestHandler<GetPurchaseOrderItemsByOrderQuery, List<PurchaseOrderItemDto>>
{
    private readonly IAppDbContext _context;

    public GetPurchaseOrderItemsByOrderQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PurchaseOrderItemDto>> Handle(GetPurchaseOrderItemsByOrderQuery request, CancellationToken cancellationToken)
    {
        return await _context.PurchaseOrderItems
            .AsNoTracking()
            .Where(i => i.PurchaseOrderId == request.PurchaseOrderId)
            .OrderBy(i => i.Id)
            .Select(i => new PurchaseOrderItemDto
            {
                Id = i.Id,
                PurchaseOrderId = i.PurchaseOrderId,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                QuantityArrived = i.QuantityArrived,
                FaultyQuantityReceived = i.FaultyQuantityReceived,
                UnitCost = i.UnitCost,
                TotalCost = i.UnitCost * i.QuantityArrived
            })
            .ToListAsync(cancellationToken);
    }
}