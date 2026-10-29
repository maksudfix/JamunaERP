using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Common.Models;
using JamunaERP.Application.PurchaseOrders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.PurchaseOrders.Queries.GetPurchaseOrdersPaged;

public record GetPurchaseOrdersPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    int? DealerId = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<PagedResult<PurchaseOrderDto>>;

public class GetPurchaseOrdersPagedQueryHandler
    : IRequestHandler<GetPurchaseOrdersPagedQuery,
        PagedResult<PurchaseOrderDto>>
{
    private readonly IAppDbContext _context;

    public GetPurchaseOrdersPagedQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PurchaseOrderDto>> Handle(
        GetPurchaseOrdersPagedQuery request,
        CancellationToken ct)
    {
        var (page, pageSize) =
            Paging.Normalize(request.Page, request.PageSize);

        var query = _context.PurchaseOrders
            .AsNoTracking()
            .AsQueryable();

        if (request.DealerId.HasValue)
        {
            query = query.Where(p =>
                p.DealerId == request.DealerId.Value);
        }

        // Inclusive start date.
        if (request.From.HasValue)
        {
            var startDate = request.From.Value.Date;

            query = query.Where(p =>
                p.ArrivalDate >= startDate);
        }

        // Inclusive end date.
        if (request.To.HasValue)
        {
            var endExclusive = request.To.Value.Date.AddDays(1);

            query = query.Where(p =>
                p.ArrivalDate < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();

            query = query.Where(p =>
                p.DealerInvoiceNo.Contains(term) ||
                p.Dealer.Name.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.ArrivalDate)
            .ThenByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            .ToListAsync(ct);

        return new PagedResult<PurchaseOrderDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}