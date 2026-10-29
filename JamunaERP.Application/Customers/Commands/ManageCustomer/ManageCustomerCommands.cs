using FluentValidation;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Customers.Commands.ManageCustomer;

// ---------- UPDATE ----------
public record UpdateCustomerCommand(int Id, string Name, string Phone, string Address) : IRequest<bool>;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Address).MaximumLength(300);
    }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
{
    private readonly IAppDbContext _context;
    public UpdateCustomerCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Customer), request.Id);

        customer.Name = request.Name.Trim();
        customer.Phone = request.Phone.Trim();
        customer.Address = request.Address?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}

// ---------- DEACTIVATE / REACTIVATE ----------
public record SetCustomerActiveCommand(int Id, bool IsActive) : IRequest<bool>;

public class SetCustomerActiveCommandHandler : IRequestHandler<SetCustomerActiveCommand, bool>
{
    private readonly IAppDbContext _context;
    public SetCustomerActiveCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(SetCustomerActiveCommand request, CancellationToken ct)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Customer), request.Id);

        if (!request.IsActive)
        {
            var hasUnpaidInstallments = await _context.InstallmentPlans
                .AnyAsync(i => i.Sale.CustomerId == request.Id
                               && i.Status != InstallmentStatus.Paid, ct);

            if (hasUnpaidInstallments)
                throw new BusinessRuleException(
                    "Cannot deactivate a customer who still has unpaid installments.");
        }

        customer.IsActive = request.IsActive;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}