using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.PurchaseOrders.Commands.CancelPurchaseOrder;

public record CancelPurchaseOrderCommand(int Id) : IRequest<bool>;

public class CancelPurchaseOrderCommandHandler : IRequestHandler<CancelPurchaseOrderCommand, bool>
{
    private readonly IAppDbContext _context;

    public CancelPurchaseOrderCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(CancelPurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        if (order.IsReceived)
            throw new BusinessRuleException(
                "Cannot cancel a purchase order that has already been received. Use Dealer Returns instead.");

        _context.PurchaseOrderItems.RemoveRange(order.Items);
        _context.PurchaseOrders.Remove(order);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}