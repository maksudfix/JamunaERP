using FluentValidation;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.PurchaseOrders.Commands.ReceivePurchaseOrder
{
    public record ReceivePurchaseOrderCommand(
        int PurchaseOrderId,
        StockLocationType Location = StockLocationType.GoDown
    ) : IRequest<bool>;

    public class ReceivePurchaseOrderCommandValidator : AbstractValidator<ReceivePurchaseOrderCommand>
    {
        public ReceivePurchaseOrderCommandValidator()
        {
            RuleFor(x => x.PurchaseOrderId).GreaterThan(0);
            RuleFor(x => x.Location).IsInEnum();
        }
    }

    public class ReceivePurchaseOrderCommandHandler : IRequestHandler<ReceivePurchaseOrderCommand, bool>
    {
        private readonly IAppDbContext _context;

        public ReceivePurchaseOrderCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(ReceivePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.PurchaseOrders
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken)
                ?? throw new NotFoundException(nameof(PurchaseOrder), request.PurchaseOrderId);

            if (order.IsReceived)
                throw new BusinessRuleException("This purchase order has already been received.");

            if (order.Items.Count == 0)
                throw new BusinessRuleException("This purchase order has no items to receive.");

            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            // Serial-less pooled rows for this location (tracked -> RowVersion enforced)
            var existing = await _context.StockItems
                .Where(s => productIds.Contains(s.ProductId)
                            && s.Location == request.Location
                            && s.SerialNumber == null)
                .ToListAsync(cancellationToken);

            foreach (var item in order.Items)
            {
                if (item.FaultyQuantityReceived > item.QuantityArrived)
                    throw new BusinessRuleException(
                        $"Faulty quantity exceeds arrived quantity for product {item.ProductId}.");

                // Latest purchase cost becomes the product's current cost (used for CostPrice on new sales)
                if (item.UnitCost > 0 && products.TryGetValue(item.ProductId, out var product))
                    product.BuyingPrice = item.UnitCost;

                var good = item.QuantityArrived - item.FaultyQuantityReceived;

                AddToPool(existing, item.ProductId, request.Location, isFaulty: false, good);
                AddToPool(existing, item.ProductId, request.Location, isFaulty: true, item.FaultyQuantityReceived);
            }

            order.IsReceived = true;
            order.ReceivedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private void AddToPool(List<StockItem> pool, int productId, StockLocationType location, bool isFaulty, int qty)
        {
            if (qty <= 0) return;

            var row = pool.FirstOrDefault(s => s.ProductId == productId && s.IsFaulty == isFaulty);
            if (row is null)
            {
                row = new StockItem
                {
                    ProductId = productId,
                    Location = location,
                    IsFaulty = isFaulty,
                    Quantity = 0
                };
                _context.StockItems.Add(row);
                pool.Add(row);
            }

            row.Quantity += qty;
        }
    }
}