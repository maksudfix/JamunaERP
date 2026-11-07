using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Common.Models;
using JamunaERP.Application.WarrantyServices.DTOs;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.WarrantyServices.Queries.GetWarrantyServicesPaged;

public record GetWarrantyServicesPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    WarrantyStatus? Status = null
) : IRequest<PagedResult<WarrantyServiceDto>>;

public class GetWarrantyServicesPagedQueryHandler
    : IRequestHandler<GetWarrantyServicesPagedQuery, PagedResult<WarrantyServiceDto>>
{
    private readonly IAppDbContext _context;

    public GetWarrantyServicesPagedQueryHandler(IAppDbContext context) => _context = context;

    public async Task<PagedResult<WarrantyServiceDto>> Handle(GetWarrantyServicesPagedQuery request, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);
        var query = _context.WarrantyServices.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(w => w.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(w => w.ProductSerialNumber.Contains(term)
                                     || w.SaleItem.Sale.InvoiceNo.Contains(term)
                                     || w.SaleItem.Sale.Customer.Name.Contains(term)
                                     || w.SaleItem.Product.Name.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(w => w.ComplaintDate).ThenByDescending(w => w.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WarrantyServiceDto
            {
                Id = w.Id,
                SaleItemId = w.SaleItemId,
                InvoiceNo = w.SaleItem.Sale.InvoiceNo,
                CustomerId = w.SaleItem.Sale.CustomerId,
                CustomerName = w.SaleItem.Sale.Customer.Name,
                ProductName = w.SaleItem.Product.Name,
                ProductSerialNumber = w.ProductSerialNumber,
                ComplaintDate = w.ComplaintDate,
                IssueDescription = w.IssueDescription,
                Status = w.Status,
                SentBackToDealerForRepair = w.SentBackToDealerForRepair,
                RequiredParts = w.RequiredParts
            })
            .ToListAsync(ct);

        return new PagedResult<WarrantyServiceDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }
}