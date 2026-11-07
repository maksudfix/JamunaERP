using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.DealerReturns.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.DealerReturns.Queries.GetDealerReturns;

public record GetDealerReturnsQuery(int DealerId) : IRequest<List<DealerReturnDto>>;

public class GetDealerReturnsQueryHandler : IRequestHandler<GetDealerReturnsQuery, List<DealerReturnDto>>
{
    private readonly IAppDbContext _context;

    public GetDealerReturnsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DealerReturnDto>> Handle(GetDealerReturnsQuery request, CancellationToken ct)
    {
        return await _context.DealerReturns
            .AsNoTracking()
            .Where(r => r.DealerId == request.DealerId)
            .OrderByDescending(r => r.ReturnDate)
            .Select(r => new DealerReturnDto
            {
                Id = r.Id,
                DealerId = r.DealerId,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                Quantity = r.Quantity,
                CreditAmount = r.CreditAmount,
                Reason = r.Reason,
                ReturnDate = r.ReturnDate
            })
            .ToListAsync(ct);
    }
}