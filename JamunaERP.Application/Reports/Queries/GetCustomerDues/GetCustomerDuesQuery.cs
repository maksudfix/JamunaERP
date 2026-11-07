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
        return await _context.Sales
            .AsNoTracking()
            .Where(s => (s.TotalAmount - s.ReturnedAmount) - (s.PaidAmount - s.RefundedAmount) > 0)
            .GroupBy(s => new { s.CustomerId, s.Customer.Name, s.Customer.Phone })
            .Select(g => new CustomerDueDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.Name,
                Phone = g.Key.Phone,
                SalesWithDue = g.Count(),
                TotalDue = g.Sum(s => (s.TotalAmount - s.ReturnedAmount) - (s.PaidAmount - s.RefundedAmount))
            })
            .OrderByDescending(c => c.TotalDue)
            .ToListAsync(ct);
    }
}