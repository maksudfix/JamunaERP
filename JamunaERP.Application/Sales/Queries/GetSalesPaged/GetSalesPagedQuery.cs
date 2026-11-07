using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Common.Models;
using JamunaERP.Application.Sales.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Sales.Queries.GetSalesPaged;

public record GetSalesPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,          // invoice no, customer name or phone
    int? CustomerId = null,
    DateTime? From = null,
    DateTime? To = null,
    bool OnlyDue = false
) : IRequest<PagedResult<SaleDto>>;

public class GetSalesPagedQueryHandler : IRequestHandler<GetSalesPagedQuery, PagedResult<SaleDto>>
{
    private readonly IAppDbContext _context;

    public GetSalesPagedQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SaleDto>> Handle(GetSalesPagedQuery request, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);
        var query = _context.Sales.AsNoTracking().AsQueryable();

        if (request.CustomerId.HasValue)
            query = query.Where(s => s.CustomerId == request.CustomerId.Value);

        if (request.From.HasValue)
        {
            var startUtc = ShopTime.StartOfDayUtc(request.From.Value);
            query = query.Where(s => s.CreatedAt >= startUtc);
        }

        if (request.To.HasValue)
        {
            var endUtc = ShopTime.StartOfDayUtc(request.To.Value.AddDays(1));
            query = query.Where(s => s.CreatedAt < endUtc);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(s => s.InvoiceNo.Contains(term)
                                     || s.Customer.Name.Contains(term)
                                     || s.Customer.Phone.Contains(term));
        }

        if (request.OnlyDue)
            query = query.Where(s => (s.TotalAmount - s.ReturnedAmount) - (s.PaidAmount - s.RefundedAmount) > 0);

        var total = await query.CountAsync(ct);

        // Items are left empty in the list view; use the invoice or sale-items endpoints for lines
        var items = await query
            .OrderByDescending(s => s.CreatedAt).ThenByDescending(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SaleDto
            {
                Id = s.Id,
                InvoiceNo = s.InvoiceNo,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer.Name,
                SaleDate = s.CreatedAt,
                TotalAmount = s.TotalAmount,
                PaidAmount = s.PaidAmount,
                ReturnedAmount = s.ReturnedAmount,
                RefundedAmount = s.RefundedAmount,
                BalanceDue = (s.TotalAmount - s.ReturnedAmount) - (s.PaidAmount - s.RefundedAmount),
                IsInstallment = s.IsInstallment,
                DiscountAmount = s.DiscountAmount,
                NetAmount = s.TotalAmount - s.ReturnedAmount
            })
            .ToListAsync(ct);

        return new PagedResult<SaleDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }
}