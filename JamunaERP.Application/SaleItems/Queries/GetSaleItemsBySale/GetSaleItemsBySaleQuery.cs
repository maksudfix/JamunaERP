using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Sales.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.SaleItems.Queries.GetSaleItemsBySale;

public record GetSaleItemsBySaleQuery(int SaleId) : IRequest<List<SaleItemDto>>;

public class GetSaleItemsBySaleQueryHandler : IRequestHandler<GetSaleItemsBySaleQuery, List<SaleItemDto>>
{
    private readonly IAppDbContext _context;

    public GetSaleItemsBySaleQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SaleItemDto>> Handle(GetSaleItemsBySaleQuery request, CancellationToken cancellationToken)
    {
        return await _context.SaleItems
            .AsNoTracking()
            .Where(i => i.SaleId == request.SaleId)
            .OrderBy(i => i.Id)
            .Select(i => new SaleItemDto
            {
                Id = i.Id,
                SaleId = i.SaleId,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.Quantity * i.UnitPrice
            })
            .ToListAsync(cancellationToken);
    }
}