using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Common.Models;
using JamunaERP.Application.Dealers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Dealers.Queries.GetDealersPaged;

public record GetDealersPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool IncludeInactive = false
) : IRequest<PagedResult<DealerDto>>;

public class GetDealersPagedQueryHandler : IRequestHandler<GetDealersPagedQuery, PagedResult<DealerDto>>
{
    private readonly IAppDbContext _context;

    public GetDealersPagedQueryHandler(IAppDbContext context) => _context = context;

    public async Task<PagedResult<DealerDto>> Handle(GetDealersPagedQuery request, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);

        var query = _context.Dealers.AsNoTracking()
            .Where(d => request.IncludeInactive || d.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(d => d.Name.Contains(term)
                                     || d.ContactPerson.Contains(term)
                                     || d.Phone.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(d => d.Name).ThenBy(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DealerDto
            {
                Id = d.Id,
                Name = d.Name,
                ContactPerson = d.ContactPerson,
                Phone = d.Phone,
                Address = d.Address,
                PurchaseOrderCount = d.PurchaseOrders.Count,
                IsActive = d.IsActive
            })
            .ToListAsync(ct);

        return new PagedResult<DealerDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }
}