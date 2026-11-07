using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.SaleReturns.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.SaleReturns.Queries.GetSaleReturns;

// SaleId = null -> all returns; otherwise only the returns of that sale
public record GetSaleReturnsQuery(int? SaleId = null) : IRequest<List<SaleReturnDto>>;

public class GetSaleReturnsQueryHandler : IRequestHandler<GetSaleReturnsQuery, List<SaleReturnDto>>
{
    private readonly IAppDbContext _context;

    public GetSaleReturnsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SaleReturnDto>> Handle(GetSaleReturnsQuery request, CancellationToken ct)
    {
        return await _context.SaleReturns
            .AsNoTracking()
            .Where(r => request.SaleId == null || r.SaleId == request.SaleId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new SaleReturnDto
            {
                Id = r.Id,
                ReturnNo = r.ReturnNo,
                SaleId = r.SaleId,
                InvoiceNo = r.Sale.InvoiceNo,
                CustomerName = r.Sale.Customer.Name,
                ReturnDate = r.CreatedAt,
                Location = r.Location,
                Reason = r.Reason,
                ReturnedValue = r.ReturnedValue,
                OutstandingReduced = r.OutstandingReduced,
                RefundAmount = r.RefundAmount,
                Items = r.Items.Select(i => new SaleReturnItemDto
                {
                    SaleItemId = i.SaleItemId,
                    ProductName = i.SaleItem.Product.Name,
                    Quantity = i.Quantity,
                    FaultyQuantity = i.FaultyQuantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.UnitPrice * i.Quantity
                }).ToList()
            })
            .ToListAsync(ct);
    }
}