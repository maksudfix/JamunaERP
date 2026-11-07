using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Reports.DTOs;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Reports.Queries.GetDailyCashBook;

public record GetDailyCashBookQuery(DateTime? Date = null) : IRequest<DailyCashBookReportDto>;

public class GetDailyCashBookQueryHandler : IRequestHandler<GetDailyCashBookQuery, DailyCashBookReportDto>
{
    private readonly IAppDbContext _context;

    public GetDailyCashBookQueryHandler(IAppDbContext context) => _context = context;

    public async Task<DailyCashBookReportDto> Handle(GetDailyCashBookQuery request, CancellationToken ct)
    {
        var targetDate = (request.Date ?? ShopTime.Today).Date;
        var startUtc = ShopTime.StartOfDayUtc(targetDate);
        var endUtc = ShopTime.StartOfDayUtc(targetDate.AddDays(1));

        var payments = await _context.Payments.AsNoTracking()
            .Where(p => p.Date >= startUtc && p.Date < endUtc)
            .Select(p => new
            {
                p.Date,
                p.Amount,
                p.Method,
                p.MobileBankingProvider,
                p.TransactionId,
                p.ReceiptNo,
                InvoiceNo = p.Sale.InvoiceNo
            })
            .ToListAsync(ct);

        var expenses = await _context.Expenses.AsNoTracking()
            .Where(e => e.ExpenseDate >= targetDate && e.ExpenseDate < targetDate.AddDays(1))
            .Select(e => new { e.CreatedAt, e.Title, e.Category, e.Amount })
            .ToListAsync(ct);

        var dealerPayments = await _context.DealerPayments.AsNoTracking()
            .Where(dp => dp.Date >= startUtc && dp.Date < endUtc)
            .Select(dp => new { dp.Date, DealerName = dp.Dealer.Name, dp.Amount, dp.Notes })
            .ToListAsync(ct);

        var salaryPayments = await _context.EmployeePayments.AsNoTracking()
            .Where(ep => ep.PaymentDate >= startUtc && ep.PaymentDate < endUtc && ep.Status == PaymentStatus.Paid)
            .Select(ep => new { ep.PaymentDate, ep.EmployeeId, ep.NetPaidAmount, ep.PaymentMonth, ep.PaymentYear })
            .ToListAsync(ct);

        var entries = new List<CashBookEntryDto>();

        foreach (var p in payments)
        {
            var isRefund = p.Amount < 0;
            var methodLabel = !string.IsNullOrWhiteSpace(p.MobileBankingProvider)
                ? $"{p.Method} ({p.MobileBankingProvider})"
                : p.Method;

            entries.Add(new CashBookEntryDto
            {
                TimeUtc = p.Date,
                Type = isRefund ? "Refund" : "SalePayment",
                Reference = $"{p.InvoiceNo} ({p.ReceiptNo ?? "N/A"})",
                Method = methodLabel,
                TransactionId = p.TransactionId,
                CashIn = isRefund ? 0m : p.Amount,
                CashOut = isRefund ? -p.Amount : 0m
            });
        }

        foreach (var e in expenses)
        {
            entries.Add(new CashBookEntryDto
            {
                TimeUtc = e.CreatedAt,
                Type = "Expense",
                Reference = $"{e.Category}: {e.Title}",
                Method = "Cash",
                CashIn = 0m,
                CashOut = e.Amount
            });
        }

        foreach (var dp in dealerPayments)
        {
            entries.Add(new CashBookEntryDto
            {
                TimeUtc = dp.Date,
                Type = "DealerPayment",
                Reference = dp.DealerName,
                Method = "Cash",
                CashIn = 0m,
                CashOut = dp.Amount
            });
        }

        foreach (var sp in salaryPayments)
        {
            entries.Add(new CashBookEntryDto
            {
                TimeUtc = sp.PaymentDate,
                Type = "SalaryPayment",
                Reference = $"Employee {sp.EmployeeId} ({sp.PaymentMonth}/{sp.PaymentYear})",
                Method = "Cash",
                CashIn = 0m,
                CashOut = sp.NetPaidAmount
            });
        }

        var positivePayments = payments.Where(p => p.Amount > 0).ToList();
        var refunds = payments.Where(p => p.Amount < 0).ToList();

        return new DailyCashBookReportDto
        {
            Date = targetDate,
            TotalCashIn = positivePayments.Sum(p => p.Amount),
            TotalRefundsOut = refunds.Sum(p => -p.Amount),
            TotalExpensesOut = expenses.Sum(e => e.Amount),
            TotalDealerPaymentsOut = dealerPayments.Sum(dp => dp.Amount),
            TotalSalariesOut = salaryPayments.Sum(sp => sp.NetPaidAmount),
            InflowByMethod = positivePayments
                .GroupBy(p => p.Method)
                .Select(g => new MethodTotalDto { Method = g.Key, Amount = g.Sum(x => x.Amount) })
                .OrderByDescending(m => m.Amount)
                .ToList(),
            InflowByMobileProvider = positivePayments
                .Where(p => !string.IsNullOrWhiteSpace(p.MobileBankingProvider))
                .GroupBy(p => p.MobileBankingProvider!)
                .Select(g => new MethodTotalDto { Method = g.Key, Amount = g.Sum(x => x.Amount) })
                .OrderByDescending(m => m.Amount)
                .ToList(),
            Entries = entries.OrderBy(e => e.TimeUtc).ToList()
        };
    }
}