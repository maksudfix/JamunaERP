using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Reports.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Reports.Queries.GetCustomerDues;

public record GetCustomerDuesQuery : IRequest<List<CustomerDueDto>>;

public class GetCustomerDuesQueryHandler : IRequestHandler<GetCustomerDuesQuery, List<CustomerDueDto>>
{
    private readonly IAppDbContext _context;

    public GetCustomerDuesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDueDto>> Handle(GetCustomerDuesQuery request, CancellationToken ct)
    {
        var rows = await _context.Sales.AsNoTracking()
            .Where(s => (s.TotalAmount - s.ReturnedAmount) - (s.PaidAmount - s.RefundedAmount) > 0)
            .Select(s => new
            {
                s.CustomerId,
                Name = s.Customer.Name,
                Phone = s.Customer.Phone,
                Due = (s.TotalAmount - s.ReturnedAmount) - (s.PaidAmount - s.RefundedAmount)
            })
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.CustomerId)
            .Select(g => new CustomerDueDto
            {
                CustomerId = g.Key,
                CustomerName = g.First().Name,
                Phone = g.First().Phone,
                SalesWithDue = g.Count(),
                TotalDue = g.Sum(x => x.Due)
            })
            .OrderByDescending(c => c.TotalDue)
            .ToList();
    }
}