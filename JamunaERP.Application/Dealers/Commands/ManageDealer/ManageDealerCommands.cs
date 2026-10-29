using FluentValidation;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Dealers.Commands.ManageDealer;

// ---------- UPDATE ----------
public record UpdateDealerCommand(
    int Id, string Name, string ContactPerson, string Phone, string Address) : IRequest<bool>;

public class UpdateDealerCommandValidator : AbstractValidator<UpdateDealerCommand>
{
    public UpdateDealerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactPerson).MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Address).MaximumLength(300);
    }
}

public class UpdateDealerCommandHandler : IRequestHandler<UpdateDealerCommand, bool>
{
    private readonly IAppDbContext _context;
    public UpdateDealerCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateDealerCommand request, CancellationToken ct)
    {
        var dealer = await _context.Dealers.FirstOrDefaultAsync(d => d.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Dealer), request.Id);

        dealer.Name = request.Name.Trim();
        dealer.ContactPerson = request.ContactPerson?.Trim() ?? string.Empty;
        dealer.Phone = request.Phone.Trim();
        dealer.Address = request.Address?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}

// ---------- DEACTIVATE / REACTIVATE ----------
public record SetDealerActiveCommand(int Id, bool IsActive) : IRequest<bool>;

public class SetDealerActiveCommandHandler : IRequestHandler<SetDealerActiveCommand, bool>
{
    private readonly IAppDbContext _context;
    public SetDealerActiveCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(SetDealerActiveCommand request, CancellationToken ct)
    {
        var dealer = await _context.Dealers.FirstOrDefaultAsync(d => d.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Dealer), request.Id);

        if (!request.IsActive)
        {
            var hasPendingOrders = await _context.PurchaseOrders
                .AnyAsync(p => p.DealerId == request.Id && !p.IsReceived, ct);

            if (hasPendingOrders)
                throw new BusinessRuleException(
                    "Cannot deactivate a dealer who has purchase orders that are not received yet.");
        }

        dealer.IsActive = request.IsActive;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}