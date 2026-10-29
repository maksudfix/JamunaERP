using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.StockItems.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JamunaERP.Application.StockItems.Queries.GetAllStockItems
{
    public record GetAllStockItemsQuery : IRequest<List<StockItemDto>>;

    public class GetAllStockItemsQueryHandler : IRequestHandler<GetAllStockItemsQuery, List<StockItemDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllStockItemsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StockItemDto>> Handle(GetAllStockItemsQuery request, CancellationToken cancellationToken)
        {
            return await _context.StockItems
                .Include(s => s.Product)
                .Select(s => new StockItemDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = s.Product != null ? s.Product.Name : string.Empty,
                    SerialNumber = s.SerialNumber ?? string.Empty,
                    Location = s.Location.ToString(),
                    IsAvailable = s.Quantity > 0 && !s.IsFaulty
                })
                .ToListAsync(cancellationToken);
        }
    }
}