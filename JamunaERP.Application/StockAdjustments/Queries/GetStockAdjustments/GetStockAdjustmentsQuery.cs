using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.StockAdjustments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.StockAdjustments.Queries.GetStockAdjustments;

public record GetStockAdjustmentsQuery(int? ProductId) : IRequest<List<StockAdjustmentDto>>;

public class GetStockAdjustmentsQueryHandler : IRequestHandler<GetStockAdjustmentsQuery, List<StockAdjustmentDto>>
{
    private readonly IAppDbContext _context;

    public GetStockAdjustmentsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockAdjustmentDto>> Handle(GetStockAdjustmentsQuery request, CancellationToken ct)
    {
        var query = _context.StockAdjustments.AsNoTracking().AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(a => a.ProductId == request.ProductId.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new StockAdjustmentDto
            {
                Id = a.Id,
                ProductId = a.ProductId,
                ProductName = a.Product.Name,
                Location = a.Location.ToString(),
                IsFaulty = a.IsFaulty,
                QuantityChange = a.QuantityChange,
                Reason = a.Reason,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.CreatedBy
            })
            .ToListAsync(ct);
    }
}