using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.DealerPayments.DTOs;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.DealerPayments.Queries.GetDealerBalance;

public record GetDealerBalanceQuery(int DealerId) : IRequest<DealerBalanceDto>;

public class GetDealerBalanceQueryHandler : IRequestHandler<GetDealerBalanceQuery, DealerBalanceDto>
{
    private readonly IAppDbContext _context;

    public GetDealerBalanceQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DealerBalanceDto> Handle(GetDealerBalanceQuery request, CancellationToken cancellationToken)
    {
        var dealer = await _context.Dealers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DealerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Dealer), request.DealerId);

        var purchased = await _context.PurchaseOrderItems
            .Where(i => i.PurchaseOrder.DealerId == request.DealerId && i.PurchaseOrder.IsReceived)
            .SumAsync(i => i.UnitCost * i.QuantityArrived, cancellationToken);

        var paid = await _context.DealerPayments
            .Where(p => p.DealerId == request.DealerId)
            .SumAsync(p => p.Amount, cancellationToken);

        var returned = await _context.DealerReturns
            .Where(r => r.DealerId == request.DealerId)
            .SumAsync(r => r.CreditAmount, cancellationToken);

        return new DealerBalanceDto
        {
            DealerId = dealer.Id,
            DealerName = dealer.Name,
            TotalPurchased = purchased,
            TotalPaid = paid,
            TotalReturned = returned
        };
    }
}