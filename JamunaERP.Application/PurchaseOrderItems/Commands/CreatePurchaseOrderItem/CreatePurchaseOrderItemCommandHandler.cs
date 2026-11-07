using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.PurchaseOrderItems.Commands.CreatePurchaseOrderItem
{
    public class CreatePurchaseOrderItemCommandHandler : IRequestHandler<CreatePurchaseOrderItemCommand, int>
    {
        private readonly IAppDbContext _context;

        public CreatePurchaseOrderItemCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreatePurchaseOrderItemCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken)
                ?? throw new NotFoundException(nameof(PurchaseOrder), request.PurchaseOrderId);

            if (order.IsReceived)
                throw new BusinessRuleException("Items cannot be added to a purchase order that has already been received.");

            var productExists = await _context.Products
                .AnyAsync(p => p.Id == request.ProductId, cancellationToken);
            if (!productExists)
                throw new NotFoundException(nameof(Product), request.ProductId);

            var item = new PurchaseOrderItem
            {
                PurchaseOrderId = request.PurchaseOrderId,
                ProductId = request.ProductId,
                QuantityArrived = request.QuantityArrived,
                FaultyQuantityReceived = request.FaultyQuantityReceived,
                UnitCost = request.UnitCost
            };

            _context.PurchaseOrderItems.Add(item);
            await _context.SaveChangesAsync(cancellationToken);

            return item.Id;
        }
    }
}