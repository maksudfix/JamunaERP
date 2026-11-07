using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.DealerPayments.Commands.RecordDealerPayment
{
    public record RecordDealerPaymentCommand(
        int DealerId,
        decimal Amount,
        DateTime? Date,
        string? Notes
    ) : IRequest<int>;

    public class RecordDealerPaymentCommandHandler : IRequestHandler<RecordDealerPaymentCommand, int>
    {
        private readonly IAppDbContext _context;

        public RecordDealerPaymentCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(RecordDealerPaymentCommand request, CancellationToken cancellationToken)
        {
            var dealerExists = await _context.Dealers
                .AnyAsync(d => d.Id == request.DealerId, cancellationToken);

            if (!dealerExists)
                throw new NotFoundException(nameof(Dealer), request.DealerId);

            var payment = new DealerPayment
            {
                DealerId = request.DealerId,
                Amount = request.Amount,
                Date = request.Date ?? DateTime.UtcNow,
                Notes = request.Notes?.Trim()
            };

            _context.DealerPayments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);

            return payment.Id;
        }
    }
}