using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.StockItems.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JamunaERP.Application.StockItems.Queries.GetStockByProduct
{
    public record GetStockByProductQuery(int ProductId) : IRequest<List<StockItemDto>>;

    public class GetStockByProductQueryHandler : IRequestHandler<GetStockByProductQuery, List<StockItemDto>>
    {
        private readonly IAppDbContext _context;

        public GetStockByProductQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StockItemDto>> Handle(GetStockByProductQuery request, CancellationToken cancellationToken)
        {
            return await _context.StockItems
                .Include(s => s.Product)
                .Where(s => s.ProductId == request.ProductId)
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