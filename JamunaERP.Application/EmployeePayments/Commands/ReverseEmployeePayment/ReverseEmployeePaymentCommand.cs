using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.EmployeePayments.Commands.ReverseEmployeePayment;

public record ReverseEmployeePaymentCommand(int Id) : IRequest<bool>;

public class ReverseEmployeePaymentCommandHandler : IRequestHandler<ReverseEmployeePaymentCommand, bool>
{
    private readonly IAppDbContext _context;

    public ReverseEmployeePaymentCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(ReverseEmployeePaymentCommand request, CancellationToken ct)
    {
        var payment = await _context.EmployeePayments
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(EmployeePayment), request.Id);

        _context.EmployeePayments.Remove(payment); // Frees up the (EmployeeId, Month, Year) unique slot & logs to AuditLogs
        await _context.SaveChangesAsync(ct);
        return true;
    }
}