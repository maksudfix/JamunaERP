using FluentValidation;
using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Reports.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Reports.Queries.GetDailySalesReport;

public record GetDailySalesReportQuery(DateTime From, DateTime To) : IRequest<DailySalesReportDto>;

public class GetDailySalesReportQueryValidator : AbstractValidator<GetDailySalesReportQuery>
{
    public GetDailySalesReportQueryValidator()
    {
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("'To' must not be before 'From'.");
        RuleFor(x => x).Must(x => (x.To.Date - x.From.Date).TotalDays <= 366)
            .WithMessage("The date range cannot exceed one year.");
    }
}

public class GetDailySalesReportQueryHandler : IRequestHandler<GetDailySalesReportQuery, DailySalesReportDto>
{
    private readonly IAppDbContext _context;

    public GetDailySalesReportQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DailySalesReportDto> Handle(GetDailySalesReportQuery request, CancellationToken ct)
    {
        var startUtc = ShopTime.StartOfDayUtc(request.From);
        var endUtc = ShopTime.StartOfDayUtc(request.To.AddDays(1));

        var sales = await _context.Sales.AsNoTracking()
            .Where(s => s.CreatedAt >= startUtc && s.CreatedAt < endUtc)
            .Select(s => new { s.CreatedAt, s.TotalAmount })
            .ToListAsync(ct);

        var returns = await _context.SaleReturns.AsNoTracking()
            .Where(r => r.CreatedAt >= startUtc && r.CreatedAt < endUtc)
            .Select(r => new { r.CreatedAt, r.ReturnedValue })
            .ToListAsync(ct);

        var payments = await _context.Payments.AsNoTracking()
            .Where(p => p.Date >= startUtc && p.Date < endUtc)
            .Select(p => new { p.Date, p.Amount, p.Method })
            .ToListAsync(ct);

        // Group by shop-local day in memory
        var rows = new SortedDictionary<DateTime, DailySalesRowDto>();

        DailySalesRowDto RowFor(DateTime utc)
        {
            var day = ShopTime.ToLocal(utc).Date;
            if (!rows.TryGetValue(day, out var row))
            {
                row = new DailySalesRowDto { Date = day };
                rows[day] = row;
            }
            return row;
        }

        foreach (var s in sales)
        {
            var row = RowFor(s.CreatedAt);
            row.SalesCount++;
            row.SalesAmount += s.TotalAmount;
        }

        foreach (var r in returns)
            RowFor(r.CreatedAt).ReturnedAmount += r.ReturnedValue;

        foreach (var p in payments)
            RowFor(p.Date).NetCollected += p.Amount;

        var list = rows.Values.ToList();

        return new DailySalesReportDto
        {
            From = request.From.Date,
            To = request.To.Date,
            Rows = list,
            ByMethod = payments
                .GroupBy(p => p.Method)
                .Select(g => new MethodTotalDto { Method = g.Key, Amount = g.Sum(x => x.Amount) })
                .OrderByDescending(m => m.Amount)
                .ToList(),
            TotalSalesCount = list.Sum(r => r.SalesCount),
            TotalSales = list.Sum(r => r.SalesAmount),
            TotalReturned = list.Sum(r => r.ReturnedAmount),
            TotalCollected = list.Sum(r => r.NetCollected)
        };
    }
}