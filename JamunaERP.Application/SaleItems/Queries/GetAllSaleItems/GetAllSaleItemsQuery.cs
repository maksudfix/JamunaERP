using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Sales.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.SaleItems.Queries.GetAllSaleItems;

public record GetAllSaleItemsQuery : IRequest<List<SaleItemDto>>;

public class GetAllSaleItemsQueryHandler : IRequestHandler<GetAllSaleItemsQuery, List<SaleItemDto>>
{
    private readonly IAppDbContext _context;

    public GetAllSaleItemsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SaleItemDto>> Handle(GetAllSaleItemsQuery request, CancellationToken cancellationToken)
    {
        return await _context.SaleItems
            .AsNoTracking()
            .OrderBy(i => i.SaleId).ThenBy(i => i.Id)
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