using FluentValidation;
using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Reports.DTOs;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Reports.Queries.GetProfitReport;

public record GetProfitReportQuery(DateTime From, DateTime To) : IRequest<ProfitReportDto>;

public class GetProfitReportQueryValidator : AbstractValidator<GetProfitReportQuery>
{
    public GetProfitReportQueryValidator()
    {
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("'To' must not be before 'From'.");
        RuleFor(x => x).Must(x => (x.To.Date - x.From.Date).TotalDays <= 366)
            .WithMessage("The date range cannot exceed one year.");
    }
}

public class GetProfitReportQueryHandler : IRequestHandler<GetProfitReportQuery, ProfitReportDto>
{
    private readonly IAppDbContext _context;

    public GetProfitReportQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ProfitReportDto> Handle(GetProfitReportQuery request, CancellationToken ct)
    {
        var startUtc = ShopTime.StartOfDayUtc(request.From);
        var endUtc = ShopTime.StartOfDayUtc(request.To.AddDays(1));

        var sales = await _context.Sales.AsNoTracking()
            .Where(s => s.CreatedAt >= startUtc && s.CreatedAt < endUtc)
            .Select(s => new
            {
                s.TotalAmount,
                Items = s.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    i.CostPrice
                }).ToList()
            })
            .ToListAsync(ct);

        // Units returned per sale item (profit is attributed to the original sale date)
        var returned = await _context.SaleReturnItems.AsNoTracking()
            .Where(r => r.SaleItem.Sale.CreatedAt >= startUtc && r.SaleItem.Sale.CreatedAt < endUtc)
            .GroupBy(r => r.SaleItemId)
            .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.ItemId, x => x.Qty, ct);

        var products = new Dictionary<int, ProductProfitRowDto>();
        var withoutCost = 0;

        foreach (var sale in sales)
        {
            // Spread any sale-level discount over the lines
            var gross = sale.Items.Sum(i => i.UnitPrice * i.Quantity);
            var factor = gross == 0 ? 1m : sale.TotalAmount / gross;

            foreach (var item in sale.Items)
            {
                returned.TryGetValue(item.Id, out var returnedQty);
                var netQty = item.Quantity - returnedQty;
                if (netQty <= 0) continue;

                if (item.CostPrice == 0) withoutCost++;

                if (!products.TryGetValue(item.ProductId, out var row))
                {
                    row = new ProductProfitRowDto { ProductId = item.ProductId, ProductName = item.ProductName };
                    products[item.ProductId] = row;
                }

                row.QuantitySold += netQty;
                row.Revenue += item.UnitPrice * netQty * factor;
                row.Cost += item.CostPrice * netQty;
            }
        }

        foreach (var row in products.Values)
        {
            row.Revenue = decimal.Round(row.Revenue, 2);
            row.Profit = row.Revenue - row.Cost;
        }

        var expenseFrom = request.From.Date;
        var expenseEnd = request.To.Date.AddDays(1);

        var expenses = await _context.Expenses.AsNoTracking()
            .Where(e => e.ExpenseDate >= expenseFrom && e.ExpenseDate < expenseEnd)
            .SumAsync(e => e.Amount, ct);

        var salaries = await _context.EmployeePayments.AsNoTracking()
            .Where(p => p.PaymentDate >= startUtc && p.PaymentDate < endUtc && p.Status == PaymentStatus.Paid)
            .SumAsync(p => p.NetPaidAmount, ct);

        var revenue = products.Values.Sum(p => p.Revenue);
        var cost = products.Values.Sum(p => p.Cost);

        return new ProfitReportDto
        {
            From = request.From.Date,
            To = request.To.Date,
            Revenue = revenue,
            Cost = cost,
            GrossProfit = revenue - cost,
            Expenses = expenses,
            Salaries = salaries,
            NetProfit = revenue - cost - expenses - salaries,
            ItemsWithoutCost = withoutCost,
            Products = products.Values.OrderByDescending(p => p.Profit).ToList()
        };
    }
}