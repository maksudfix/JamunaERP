using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Common.Models;
using JamunaERP.Application.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Products.Queries.GetProductsPaged;

// IncludeCost / IncludeInactive are only true for Admin / Manager (the controller decides)
public record GetProductsPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,           // name or model no
    string? Category = null,
    bool IncludeCost = false,
    bool IncludeInactive = false
) : IRequest<PagedResult<ProductDto>>;

public class GetProductsPagedQueryHandler : IRequestHandler<GetProductsPagedQuery, PagedResult<ProductDto>>
{
    private readonly IAppDbContext _context;

    public GetProductsPagedQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsPagedQuery request, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);

        var query = _context.Products.AsNoTracking()
            .Where(p => request.IncludeInactive || p.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.ModelNo.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim();
            query = query.Where(p => p.Category == category);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name).ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ModelNo = p.ModelNo,
                Category = p.Category,
                BuyingPrice = p.BuyingPrice,
                SellingPrice = p.SellingPrice,
                WarrantyMonths = p.WarrantyMonths,
                IsActive = p.IsActive
            })
            .ToListAsync(ct);

        if (!request.IncludeCost)
        {
            foreach (var product in items)
                product.BuyingPrice = null;
        }

        return new PagedResult<ProductDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }
}