using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Sales.Commands.CreateSale
{
    public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, int>
    {
        private readonly IAppDbContext _context;

        public CreateSaleCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            // 1. Customer must exist and be active
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken)
                ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

            if (!customer.IsActive)
                throw new BusinessRuleException("This customer is deactivated and cannot be used for new sales.");

            // 2. Merge duplicate product lines and load products (must exist and be active)
            var lines = request.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            var productIds = lines.Select(l => l.ProductId).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            foreach (var id in productIds)
            {
                if (!products.TryGetValue(id, out var p))
                    throw new NotFoundException(nameof(Product), id);

                if (!p.IsActive)
                    throw new BusinessRuleException($"Product '{p.Name}' is deactivated and cannot be sold.");
            }

            // 3. Load sellable stock rows (tracked, so RowVersion is checked on save)
            var stockRows = await _context.StockItems
                .Where(s => productIds.Contains(s.ProductId)
                            && s.Location == request.Location
                            && !s.IsFaulty
                            && s.Quantity > 0)
                .OrderBy(s => s.Id) // FIFO
                .ToListAsync(cancellationToken);

            // 4. Build sale items (price comes from the DB, never from the client) and deduct stock
            var sale = new Sale
            {
                InvoiceNo = GenerateInvoiceNo(),
                CustomerId = request.CustomerId,
                IsInstallment = request.IsInstallment,
                InstallmentCount = request.IsInstallment ? request.InstallmentCount : 0
            };

            foreach (var line in lines)
            {
                var product = products[line.ProductId];
                var rows = stockRows.Where(s => s.ProductId == line.ProductId).ToList();
                var available = rows.Sum(r => r.Quantity);

                if (available < line.Quantity)
                    throw new InsufficientStockException(product.Id, product.Name, line.Quantity, available);

                var remaining = line.Quantity;
                foreach (var row in rows)
                {
                    if (remaining == 0) break;
                    var take = Math.Min(row.Quantity, remaining);
                    row.Quantity -= take;
                    remaining -= take;
                }

                var costAtSale = product.BuyingPrice;

                if (costAtSale < 0)
                {
                    throw new BusinessRuleException(
                        $"Product '{product.Name}' has an invalid buying cost.");
                }

                sale.Items.Add(new SaleItem
                {
                    ProductId = product.Id,
                    Quantity = line.Quantity,
                    UnitPrice = product.SellingPrice,
                    CostPrice = costAtSale
                });
            }

            var gross = sale.Items.Sum(i => i.UnitPrice * i.Quantity);
            if (request.DiscountAmount > gross)
                throw new BusinessRuleException("Discount cannot exceed the sale amount.");

            sale.DiscountAmount = request.DiscountAmount;
            sale.TotalAmount = gross - request.DiscountAmount; // net amount the customer must pay
            sale.PaidAmount = request.PaidAmount;

            // 5. Payment rules (need the computed total, so they live here, not in the validator)
            if (request.PaidAmount > sale.TotalAmount)
                throw new BusinessRuleException("Paid amount cannot exceed the sale total.");

            if (!request.IsInstallment && request.PaidAmount != sale.TotalAmount)
                throw new BusinessRuleException("A non-installment sale must be paid in full.");

            if (request.IsInstallment && request.PaidAmount >= sale.TotalAmount)
                throw new BusinessRuleException("An installment sale must have an outstanding balance.");

            // 6. Generate installments
            if (request.IsInstallment)
            {
                var balance = sale.TotalAmount - sale.PaidAmount;
                var count = request.InstallmentCount;
                var baseAmount = decimal.Round(balance / count, 2, MidpointRounding.ToZero);
                var firstDue = (request.FirstDueDate ?? DateTime.UtcNow.AddMonths(1)).Date;

                for (var n = 1; n <= count; n++)
                {
                    var amount = n == count ? balance - (baseAmount * (count - 1)) : baseAmount;

                    sale.Installments.Add(new InstallmentPlan
                    {
                        InstallmentNumber = n,
                        AmountDue = amount,
                        DueDate = firstDue.AddMonths(n - 1),
                        Status = InstallmentStatus.Pending
                    });
                }
            }

            // 7. Receipt for the money collected now (full payment, or the down payment)
            if (sale.PaidAmount > 0)
            {
                sale.Payments.Add(new Payment
                {
                    Amount = sale.PaidAmount,
                    Method = request.PaymentMethod,
                    ReceiptNo = ReceiptNumber.Generate(),
                    Date = DateTime.UtcNow
                });
            }

            // 8. One SaveChanges = one transaction: sale + items + installments + payment + stock deduction
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync(cancellationToken);

            return sale.Id;
        }

        private static string GenerateInvoiceNo() =>
            $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}