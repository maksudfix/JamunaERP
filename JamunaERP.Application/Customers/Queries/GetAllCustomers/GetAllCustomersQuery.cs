using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Customers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Customers.Queries.GetAllCustomers;

public record GetAllCustomersQuery(bool IncludeInactive = false) : IRequest<List<CustomerDto>>;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
{
    private readonly IAppDbContext _context;

    public GetAllCustomersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()
            .Where(c => request.IncludeInactive || c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Address = c.Address,
                CreatedAt = c.CreatedAt,
                SalesCount = c.Sales.Count,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}