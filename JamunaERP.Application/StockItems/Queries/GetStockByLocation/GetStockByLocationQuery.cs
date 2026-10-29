using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.StockItems.DTOs;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JamunaERP.Application.StockItems.Queries.GetStockByLocation
{
    public record GetStockByLocationQuery(StockLocationType Location) : IRequest<List<StockItemDto>>;

    public class GetStockByLocationQueryHandler : IRequestHandler<GetStockByLocationQuery, List<StockItemDto>>
    {
        private readonly IAppDbContext _context;

        public GetStockByLocationQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StockItemDto>> Handle(GetStockByLocationQuery request, CancellationToken cancellationToken)
        {
            return await _context.StockItems
                .Include(s => s.Product)
                .Where(s => s.Location == request.Location)
                .Select(s => new StockItemDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = s.Product != null ? s.Product.Name : string.Empty,
                    SerialNumber = s.SerialNumber ?? string.Empty,
                    Location = s.Location.ToString(), // Converts enum to string for the DTO
                    IsAvailable = s.Quantity > 0 && !s.IsFaulty
                })
                .ToListAsync(cancellationToken);
        }
    }
}