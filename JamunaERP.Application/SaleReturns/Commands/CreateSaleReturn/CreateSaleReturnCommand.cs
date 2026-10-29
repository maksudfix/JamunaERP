using FluentValidation;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.SaleReturns.Commands.CreateSaleReturn
{
    // One line of the return: which sale item, how many come back, how many of those are faulty
    public record SaleReturnLineInput(int SaleItemId, int Quantity, int FaultyQuantity = 0);

    public record CreateSaleReturnCommand(
        int SaleId,
        string Reason,
        StockLocationType Location,          // where the returned goods are put back
        List<SaleReturnLineInput> Items
    ) : IRequest<SaleReturnResult>;

    public record SaleReturnResult(
        int ReturnId,
        string ReturnNo,
        decimal ReturnedValue,
        decimal OutstandingReduced,
        decimal RefundAmount);

    public class CreateSaleReturnCommandValidator : AbstractValidator<CreateSaleReturnCommand>
    {
        public CreateSaleReturnCommandValidator()
        {
            RuleFor(x => x.SaleId).GreaterThan(0);
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Location).IsInEnum();
            RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");

            RuleForEach(x => x.Items).ChildRules(line =>
            {
                line.RuleFor(l => l.SaleItemId).GreaterThan(0);
                line.RuleFor(l => l.Quantity).GreaterThan(0);
                line.RuleFor(l => l.FaultyQuantity).GreaterThanOrEqualTo(0);
                line.RuleFor(l => l.FaultyQuantity)
                    .LessThanOrEqualTo(l => l.Quantity)
                    .WithMessage("Faulty quantity cannot exceed the returned quantity.");
            });
        }
    }

    public class CreateSaleReturnCommandHandler : IRequestHandler<CreateSaleReturnCommand, SaleReturnResult>
    {
        private readonly IAppDbContext _context;

        public CreateSaleReturnCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<SaleReturnResult> Handle(CreateSaleReturnCommand request, CancellationToken ct)
        {
            // 1. Load the sale with everything we may change
            var sale = await _context.Sales
                .Include(s => s.Items)
                .Include(s => s.Installments)
                .FirstOrDefaultAsync(s => s.Id == request.SaleId, ct)
                ?? throw new NotFoundException(nameof(Sale), request.SaleId);

            // 2. Merge duplicate lines
            var lines = request.Items
                .GroupBy(i => i.SaleItemId)
                .Select(g => new
                {
                    SaleItemId = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    FaultyQuantity = g.Sum(x => x.FaultyQuantity)
                })
                .ToList();

            // 3. How many units of each item were already returned before
            var saleItemIds = sale.Items.Select(i => i.Id).ToList();

            var alreadyReturned = await _context.SaleReturnItems
                .Where(r => saleItemIds.Contains(r.SaleItemId))
                .GroupBy(r => r.SaleItemId)
                .Select(g => new { SaleItemId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.SaleItemId, x => x.Qty, ct);

            // Pro-rate returns when a discount was given (factor is 1 when there was none)
            var gross = sale.Items.Sum(i => i.UnitPrice * i.Quantity);
            var factor = gross == 0 ? 1m : sale.TotalAmount / gross;

            // 4. Validate every line and build the return
            var saleReturn = new SaleReturn
            {
                ReturnNo = GenerateReturnNo(),
                SaleId = sale.Id,
                Location = request.Location,
                Reason = request.Reason.Trim()
            };

            decimal returnedValue = 0;
            var stockLines = new List<(int ProductId, int Good, int Faulty)>();

            foreach (var line in lines)
            {
                var saleItem = sale.Items.FirstOrDefault(i => i.Id == line.SaleItemId)
                    ?? throw new BusinessRuleException(
                        $"Sale item {line.SaleItemId} does not belong to sale {sale.InvoiceNo}.");

                if (line.FaultyQuantity > line.Quantity)
                    throw new BusinessRuleException("Faulty quantity cannot exceed the returned quantity.");

                alreadyReturned.TryGetValue(saleItem.Id, out var returnedBefore);
                var returnable = saleItem.Quantity - returnedBefore;

                if (line.Quantity > returnable)
                    throw new BusinessRuleException(
                        $"Cannot return {line.Quantity} unit(s) of sale item {saleItem.Id}: " +
                        $"only {returnable} unit(s) are still returnable.");

                returnedValue += decimal.Round(saleItem.UnitPrice * line.Quantity * factor, 2);

                saleReturn.Items.Add(new SaleReturnItem
                {
                    SaleItemId = saleItem.Id,
                    Quantity = line.Quantity,
                    FaultyQuantity = line.FaultyQuantity,
                    UnitPrice = saleItem.UnitPrice
                });

                stockLines.Add((saleItem.ProductId, line.Quantity - line.FaultyQuantity, line.FaultyQuantity));
            }

            // 5. Money: first reduce what the customer still owes, the rest is a cash refund
            var netTotal = sale.TotalAmount - sale.ReturnedAmount;   // value still "live" on this sale

            // Per-line rounding can drift by a paisa. Never return more than what is left on the sale,
            // and when this return clears every remaining unit, return exactly what is left.
            var clearsSale = sale.Items.All(i =>
                alreadyReturned.GetValueOrDefault(i.Id)
                + (lines.FirstOrDefault(l => l.SaleItemId == i.Id)?.Quantity ?? 0) == i.Quantity);

            returnedValue = clearsSale ? netTotal : Math.Min(returnedValue, netTotal);

            var netPaid = sale.PaidAmount - sale.RefundedAmount;
            var outstanding = Math.Max(0m, netTotal - netPaid);
            var outstandingReduced = Math.Min(returnedValue, outstanding);
            var refund = returnedValue - outstandingReduced;

            // 6. Cancel unpaid installments, starting from the last one
            if (outstandingReduced > 0)
            {
                var left = outstandingReduced;

                var open = sale.Installments
                    .Where(i => i.Status != InstallmentStatus.Paid && i.Status != InstallmentStatus.Cancelled)
                    .OrderByDescending(i => i.InstallmentNumber);

                foreach (var installment in open)
                {
                    if (left == 0) break;

                    var remaining = installment.AmountDue - installment.AmountPaid;
                    if (remaining <= 0) continue;

                    var cut = Math.Min(remaining, left);
                    installment.AmountDue -= cut;
                    left -= cut;

                    if (installment.AmountPaid >= installment.AmountDue)
                    {
                        installment.Status = installment.AmountPaid > 0
                            ? InstallmentStatus.Paid
                            : InstallmentStatus.Cancelled;
                    }
                }

                if (left > 0)
                    throw new BusinessRuleException(
                        "The installment schedule does not match the sale balance. Please check this sale manually.");
            }

            // 7. Put the goods back into stock (good and faulty pools, serial-less rows)
            var productIds = stockLines.Select(s => s.ProductId).Distinct().ToList();

            var pool = await _context.StockItems
                .Where(s => productIds.Contains(s.ProductId)
                            && s.Location == request.Location
                            && s.SerialNumber == null)
                .ToListAsync(ct);

            foreach (var s in stockLines)
            {
                AddToPool(pool, s.ProductId, request.Location, isFaulty: false, s.Good);
                AddToPool(pool, s.ProductId, request.Location, isFaulty: true, s.Faulty);
            }

            // 8. Refund ledger integration (Refunds go as negative payments)
            if (refund > 0)
            {
                _context.Payments.Add(new Payment
                {
                    SaleId = sale.Id,
                    Amount = -refund,
                    Method = "Refund",
                    ReceiptNo = saleReturn.ReturnNo,
                    Date = DateTime.UtcNow
                });
            }

            // 9. Update the sale header and save everything in one transaction
            sale.ReturnedAmount += returnedValue;
            sale.RefundedAmount += refund;

            saleReturn.ReturnedValue = returnedValue;
            saleReturn.OutstandingReduced = outstandingReduced;
            saleReturn.RefundAmount = refund;

            _context.SaleReturns.Add(saleReturn);
            await _context.SaveChangesAsync(ct);

            return new SaleReturnResult(
                saleReturn.Id, saleReturn.ReturnNo, returnedValue, outstandingReduced, refund);
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

        private static string GenerateReturnNo() =>
            $"RET-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}