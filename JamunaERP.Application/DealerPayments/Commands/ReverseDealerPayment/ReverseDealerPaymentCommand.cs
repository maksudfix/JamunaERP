using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.DealerPayments.Commands.ReverseDealerPayment;

public record ReverseDealerPaymentCommand(int Id) : IRequest<bool>;

public class ReverseDealerPaymentCommandHandler : IRequestHandler<ReverseDealerPaymentCommand, bool>
{
    private readonly IAppDbContext _context;

    public ReverseDealerPaymentCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(ReverseDealerPaymentCommand request, CancellationToken ct)
    {
        var payment = await _context.DealerPayments
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(DealerPayment), request.Id);

        _context.DealerPayments.Remove(payment); // AuditSaveChangesInterceptor automatically logs the Deleted snapshot
        await _context.SaveChangesAsync(ct);
        return true;
    }
}