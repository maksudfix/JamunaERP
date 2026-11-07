using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Common.Models;
using JamunaERP.Application.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Products.Queries.GetProductsPaged;

public record GetProductsPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Category = null,
    bool IncludeCost = false,
    bool IncludeInactive = false,
    string? Brand = null
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
            query = query.Where(p => p.Name.Contains(term)
                                     || p.ModelNo.Contains(term)
                                     || (p.Barcode != null && p.Barcode.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim();
            query = query.Where(p => p.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(request.Brand))
        {
            var brand = request.Brand.Trim();
            query = query.Where(p => p.Brand == brand);
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
                Brand = p.Brand,
                ModelNo = p.ModelNo,
                Category = p.Category,
                Barcode = p.Barcode,
                BuyingPrice = p.BuyingPrice,
                SellingPrice = p.SellingPrice,
                WarrantyMonths = p.WarrantyMonths,
                ReorderLevel = p.ReorderLevel,
                DefaultDealerId = p.DefaultDealerId,
                DefaultDealerName = p.DefaultDealer != null ? p.DefaultDealer.Name : null,
                AllowInstallment = p.AllowInstallment,
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