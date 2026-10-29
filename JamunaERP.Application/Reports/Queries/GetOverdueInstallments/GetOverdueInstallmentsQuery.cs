using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Reports.DTOs;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Reports.Queries.GetOverdueInstallments;

public record GetOverdueInstallmentsQuery : IRequest<List<OverdueInstallmentDto>>;

public class GetOverdueInstallmentsQueryHandler : IRequestHandler<GetOverdueInstallmentsQuery, List<OverdueInstallmentDto>>
{
    private readonly IAppDbContext _context;

    public GetOverdueInstallmentsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OverdueInstallmentDto>> Handle(GetOverdueInstallmentsQuery request, CancellationToken ct)
    {
        var today = ShopTime.Today;

        // Based on dates, not on Status: an installment stays "Pending" until someone pays it
        var rows = await _context.InstallmentPlans.AsNoTracking()
            .Where(i => i.Status != InstallmentStatus.Paid
                        && i.Status != InstallmentStatus.Cancelled
                        && i.DueDate < today
                        && i.AmountDue - i.AmountPaid > 0)
            .OrderBy(i => i.DueDate)
            .Select(i => new OverdueInstallmentDto
            {
                InstallmentId = i.Id,
                SaleId = i.SaleId,
                InvoiceNo = i.Sale.InvoiceNo,
                CustomerId = i.Sale.CustomerId,
                CustomerName = i.Sale.Customer.Name,
                Phone = i.Sale.Customer.Phone,
                InstallmentNumber = i.InstallmentNumber,
                DueDate = i.DueDate,
                Remaining = i.AmountDue - i.AmountPaid
            })
            .ToListAsync(ct);

        foreach (var row in rows)
            row.DaysOverdue = (today - row.DueDate.Date).Days;

        return rows;
    }
}