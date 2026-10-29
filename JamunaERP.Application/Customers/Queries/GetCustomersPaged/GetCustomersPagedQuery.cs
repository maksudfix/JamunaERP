using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Common.Models;
using JamunaERP.Application.Customers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Customers.Queries.GetCustomersPaged;

public record GetCustomersPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,           // name, phone or address
    bool IncludeInactive = false
) : IRequest<PagedResult<CustomerDto>>;

public class GetCustomersPagedQueryHandler : IRequestHandler<GetCustomersPagedQuery, PagedResult<CustomerDto>>
{
    private readonly IAppDbContext _context;

    public GetCustomersPagedQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersPagedQuery request, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);

        var query = _context.Customers.AsNoTracking()
            .Where(c => request.IncludeInactive || c.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(c => c.Name.Contains(term) || c.Phone.Contains(term) || c.Address.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Name).ThenBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            .ToListAsync(ct);

        return new PagedResult<CustomerDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }
}