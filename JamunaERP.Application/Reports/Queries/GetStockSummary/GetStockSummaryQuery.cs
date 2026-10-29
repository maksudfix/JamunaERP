using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Reports.DTOs;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Reports.Queries.GetStockSummary;

// LowStockThreshold: only products whose good units are at or below this number
public record GetStockSummaryQuery(int? LowStockThreshold = null) : IRequest<StockSummaryDto>;

public class GetStockSummaryQueryHandler : IRequestHandler<GetStockSummaryQuery, StockSummaryDto>
{
    private readonly IAppDbContext _context;

    public GetStockSummaryQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<StockSummaryDto> Handle(GetStockSummaryQuery request, CancellationToken ct)
    {
        var products = await _context.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.Name, p.ModelNo, p.Category, p.BuyingPrice })
            .ToListAsync(ct);

        var stock = await _context.StockItems.AsNoTracking()
            .Select(s => new { s.ProductId, s.Location, s.IsFaulty, s.Quantity })
            .ToListAsync(ct);

        var byProduct = stock.GroupBy(s => s.ProductId).ToDictionary(g => g.Key, g => g.ToList());

        var rows = new List<StockSummaryRowDto>();

        foreach (var p in products)
        {
            var list = byProduct.GetValueOrDefault(p.Id) ?? new();
            var good = list.Where(s => !s.IsFaulty).ToList();

            var row = new StockSummaryRowDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                ModelNo = p.ModelNo,
                Category = p.Category,
                Showroom1 = good.Where(s => s.Location == StockLocationType.Showroom1).Sum(s => s.Quantity),
                Showroom2 = good.Where(s => s.Location == StockLocationType.Showroom2).Sum(s => s.Quantity),
                GoDown = good.Where(s => s.Location == StockLocationType.GoDown).Sum(s => s.Quantity),
                TotalGood = good.Sum(s => s.Quantity),
                TotalFaulty = list.Where(s => s.IsFaulty).Sum(s => s.Quantity),
                BuyingPrice = p.BuyingPrice
            };
            row.StockValue = row.TotalGood * row.BuyingPrice;

            if (request.LowStockThreshold.HasValue && row.TotalGood > request.LowStockThreshold.Value)
                continue;

            rows.Add(row);
        }

        return new StockSummaryDto
        {
            Items = rows.OrderBy(r => r.TotalGood).ThenBy(r => r.ProductName).ToList(),
            TotalUnits = rows.Sum(r => r.TotalGood),
            TotalStockValue = rows.Sum(r => r.StockValue)
        };
    }
}