using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Payments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Payments.Queries.GetPaymentsBySale;

public record GetPaymentsBySaleQuery(int SaleId) : IRequest<List<PaymentDto>>;

public class GetPaymentsBySaleQueryHandler : IRequestHandler<GetPaymentsBySaleQuery, List<PaymentDto>>
{
    private readonly IAppDbContext _context;

    public GetPaymentsBySaleQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentDto>> Handle(GetPaymentsBySaleQuery request, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.SaleId == request.SaleId)
            .OrderByDescending(p => p.Date)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                SaleId = p.SaleId,
                Amount = p.Amount,
                Date = p.Date,
                Method = p.Method,
                ReceiptNo = p.ReceiptNo
            })
            .ToListAsync(cancellationToken);
    }
}