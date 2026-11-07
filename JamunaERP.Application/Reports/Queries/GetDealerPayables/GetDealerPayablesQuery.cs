using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Reports.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Reports.Queries.GetDealerPayables;

public record GetDealerPayablesQuery(bool IncludeSettled = false) : IRequest<List<DealerPayableDto>>;

public class GetDealerPayablesQueryHandler : IRequestHandler<GetDealerPayablesQuery, List<DealerPayableDto>>
{
    private readonly IAppDbContext _context;

    public GetDealerPayablesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DealerPayableDto>> Handle(GetDealerPayablesQuery request, CancellationToken ct)
    {
        var dealers = await _context.Dealers.AsNoTracking()
            .Select(d => new { d.Id, d.Name, d.Phone })
            .ToListAsync(ct);

        // Only goods actually received create a debt to the dealer
        var purchased = await _context.PurchaseOrderItems.AsNoTracking()
            .Where(i => i.PurchaseOrder.IsReceived)
            .GroupBy(i => i.PurchaseOrder.DealerId)
            .Select(g => new { DealerId = g.Key, Total = g.Sum(x => x.UnitCost * x.QuantityArrived) })
            .ToDictionaryAsync(x => x.DealerId, x => x.Total, ct);

        var returned = await _context.DealerReturns.AsNoTracking()
            .GroupBy(r => r.DealerId)
            .Select(g => new { DealerId = g.Key, Total = g.Sum(x => x.CreditAmount) })
            .ToDictionaryAsync(x => x.DealerId, x => x.Total, ct);

        var paid = await _context.DealerPayments.AsNoTracking()
            .GroupBy(p => p.DealerId)
            .Select(g => new { DealerId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.DealerId, x => x.Total, ct);

        var result = dealers.Select(d =>
        {
            var p = purchased.GetValueOrDefault(d.Id);
            var r = returned.GetValueOrDefault(d.Id);
            var pd = paid.GetValueOrDefault(d.Id);

            return new DealerPayableDto
            {
                DealerId = d.Id,
                DealerName = d.Name,
                Phone = d.Phone,
                TotalPurchased = p,
                TotalReturned = r,
                TotalPaid = pd,
                Outstanding = p - r - pd
            };
        });

        if (!request.IncludeSettled)
            result = result.Where(d => d.Outstanding != 0);

        return result.OrderByDescending(d => d.Outstanding).ToList();
    }
}