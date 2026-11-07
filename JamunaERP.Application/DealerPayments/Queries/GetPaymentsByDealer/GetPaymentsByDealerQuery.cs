using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.DealerPayments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.DealerPayments.Queries.GetPaymentsByDealer;

public record GetPaymentsByDealerQuery(int DealerId) : IRequest<List<DealerPaymentDto>>;

public class GetPaymentsByDealerQueryHandler : IRequestHandler<GetPaymentsByDealerQuery, List<DealerPaymentDto>>
{
    private readonly IAppDbContext _context;

    public GetPaymentsByDealerQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DealerPaymentDto>> Handle(GetPaymentsByDealerQuery request, CancellationToken cancellationToken)
    {
        return await _context.DealerPayments
            .AsNoTracking()
            .Where(p => p.DealerId == request.DealerId)
            .OrderByDescending(p => p.Date)
            .Select(p => new DealerPaymentDto
            {
                Id = p.Id,
                DealerId = p.DealerId,
                Amount = p.Amount,
                Date = p.Date,
                Notes = p.Notes
            })
            .ToListAsync(cancellationToken);
    }
}