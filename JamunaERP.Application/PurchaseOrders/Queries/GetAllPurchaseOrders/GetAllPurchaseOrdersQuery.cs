using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.PurchaseOrders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.PurchaseOrders.Queries.GetAllPurchaseOrders;

public record GetAllPurchaseOrdersQuery : IRequest<List<PurchaseOrderDto>>;

public class GetAllPurchaseOrdersQueryHandler : IRequestHandler<GetAllPurchaseOrdersQuery, List<PurchaseOrderDto>>
{
    private readonly IAppDbContext _context;

    public GetAllPurchaseOrdersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PurchaseOrderDto>> Handle(GetAllPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _context.PurchaseOrders
            .AsNoTracking()
            .OrderByDescending(p => p.ArrivalDate)
            .Select(p => new PurchaseOrderDto
            {
                Id = p.Id,
                DealerInvoiceNo = p.DealerInvoiceNo,
                DealerId = p.DealerId,
                DealerName = p.Dealer.Name,
                ArrivalDate = p.ArrivalDate,
                NextLotEstimatedDate = p.NextLotEstimatedDate,
                Items = p.Items.Select(i => new PurchaseOrderItemDto
                {
                    Id = i.Id,
                    PurchaseOrderId = i.PurchaseOrderId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    QuantityArrived = i.QuantityArrived,
                    FaultyQuantityReceived = i.FaultyQuantityReceived,
                    UnitCost = i.UnitCost,
                    TotalCost = i.UnitCost * i.QuantityArrived
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}