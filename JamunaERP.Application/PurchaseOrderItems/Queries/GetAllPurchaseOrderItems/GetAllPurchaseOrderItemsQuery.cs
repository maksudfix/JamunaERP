using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.PurchaseOrders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.PurchaseOrderItems.Queries.GetAllPurchaseOrderItems;

public record GetAllPurchaseOrderItemsQuery : IRequest<List<PurchaseOrderItemDto>>;

public class GetAllPurchaseOrderItemsQueryHandler : IRequestHandler<GetAllPurchaseOrderItemsQuery, List<PurchaseOrderItemDto>>
{
    private readonly IAppDbContext _context;

    public GetAllPurchaseOrderItemsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PurchaseOrderItemDto>> Handle(GetAllPurchaseOrderItemsQuery request, CancellationToken cancellationToken)
    {
        return await _context.PurchaseOrderItems
            .AsNoTracking()
            .OrderBy(i => i.PurchaseOrderId).ThenBy(i => i.Id)
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