using FluentValidation;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.PurchaseOrderItems.Commands.ManagePurchaseOrderItem;

// ---------- UPDATE PO ITEM (before receiving) ----------
public record UpdatePurchaseOrderItemCommand(
    int Id,
    int QuantityArrived,
    int FaultyQuantityReceived,
    decimal UnitCost
) : IRequest<bool>;

public class UpdatePurchaseOrderItemCommandValidator : AbstractValidator<UpdatePurchaseOrderItemCommand>
{
    public UpdatePurchaseOrderItemCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.QuantityArrived).GreaterThan(0);
        RuleFor(x => x.FaultyQuantityReceived)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.QuantityArrived)
            .WithMessage("Faulty quantity cannot exceed the quantity arrived.");
        RuleFor(x => x.UnitCost)
            .GreaterThan(0)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true);
    }
}

public class UpdatePurchaseOrderItemCommandHandler : IRequestHandler<UpdatePurchaseOrderItemCommand, bool>
{
    private readonly IAppDbContext _context;

    public UpdatePurchaseOrderItemCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(UpdatePurchaseOrderItemCommand request, CancellationToken ct)
    {
        var item = await _context.PurchaseOrderItems
            .Include(i => i.PurchaseOrder)
            .FirstOrDefaultAsync(i => i.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(PurchaseOrderItem), request.Id);

        if (item.PurchaseOrder.IsReceived)
            throw new BusinessRuleException("Cannot edit items of a purchase order that has already been received.");

        item.QuantityArrived = request.QuantityArrived;
        item.FaultyQuantityReceived = request.FaultyQuantityReceived;
        item.UnitCost = request.UnitCost;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}

// ---------- DELETE PO ITEM (before receiving) ----------
public record DeletePurchaseOrderItemCommand(int Id) : IRequest<bool>;

public class DeletePurchaseOrderItemCommandHandler : IRequestHandler<DeletePurchaseOrderItemCommand, bool>
{
    private readonly IAppDbContext _context;

    public DeletePurchaseOrderItemCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(DeletePurchaseOrderItemCommand request, CancellationToken ct)
    {
        var item = await _context.PurchaseOrderItems
            .Include(i => i.PurchaseOrder)
            .FirstOrDefaultAsync(i => i.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(PurchaseOrderItem), request.Id);

        if (item.PurchaseOrder.IsReceived)
            throw new BusinessRuleException("Cannot delete items of a purchase order that has already been received.");

        _context.PurchaseOrderItems.Remove(item);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}