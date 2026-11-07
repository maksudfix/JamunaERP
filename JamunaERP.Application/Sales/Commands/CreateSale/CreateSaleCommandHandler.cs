using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Common.Security;
using JamunaERP.Application.Contracts;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Sales.Commands.CreateSale
{
    public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, int>
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUserService? _currentUser;

        public CreateSaleCommandHandler(IAppDbContext context, ICurrentUserService? currentUser = null)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser is not null
                && !_currentUser.IsInRole(Roles.Admin)
                && !_currentUser.IsInRole(Roles.Manager)
                && _currentUser.AssignedLocation.HasValue
                && _currentUser.AssignedLocation.Value != request.Location)
            {
                throw new ForbiddenException(
                    $"You are assigned to {_currentUser.AssignedLocation.Value} and cannot create sales from {request.Location}.");
            }

            // 1. Customer must exist and be active (tracked so NID/AlternatePhone can be updated if supplied)
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken)
                ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

            if (!customer.IsActive)
                throw new BusinessRuleException("This customer is deactivated and cannot be used for new sales.");

            if (!string.IsNullOrWhiteSpace(request.CustomerNid))
                customer.NidNumber = request.CustomerNid.Trim();

            if (!string.IsNullOrWhiteSpace(request.CustomerAlternatePhone))
                customer.AlternatePhone = request.CustomerAlternatePhone.Trim();

            // When called by an authenticated API user (or when Guarantor is supplied), enforce NID & AlternatePhone on installment sales
            if (request.IsInstallment && (_currentUser?.UserId is not null || request.Guarantor is not null))
            {
                if (string.IsNullOrWhiteSpace(customer.NidNumber))
                    throw new BusinessRuleException("Customer NID number is required for installment sales.");

                if (string.IsNullOrWhiteSpace(customer.AlternatePhone))
                    throw new BusinessRuleException("Customer alternate phone number is required for installment sales.");

                if (request.Guarantor is null
                    || string.IsNullOrWhiteSpace(request.Guarantor.Name)
                    || string.IsNullOrWhiteSpace(request.Guarantor.Phone)
                    || string.IsNullOrWhiteSpace(request.Guarantor.Address)
                    || string.IsNullOrWhiteSpace(request.Guarantor.NidNumber))
                {
                    throw new BusinessRuleException(
                        "Guarantor name, phone, address, and NID number are required for installment sales.");
                }
            }

            // 2. Merge duplicate product/stock/serial lines and load products
            var lines = request.Items
                .GroupBy(i => new
                {
                    i.ProductId,
                    i.StockItemId,
                    SerialNumber = string.IsNullOrWhiteSpace(i.SerialNumber) ? null : i.SerialNumber.Trim()
                })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.StockItemId,
                    g.Key.SerialNumber,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            var productIds = lines.Select(l => l.ProductId).Distinct().ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            foreach (var id in productIds)
            {
                if (!products.TryGetValue(id, out var p))
                    throw new NotFoundException(nameof(Product), id);

                if (!p.IsActive)
                    throw new BusinessRuleException($"Product '{p.Name}' is deactivated and cannot be sold.");

                if (request.IsInstallment && !p.AllowInstallment)
                    throw new BusinessRuleException($"Product '{p.Name}' is not eligible for installment sales.");
            }

            // 3. Load sellable stock rows at this location
            var stockRows = await _context.StockItems
                .Where(s => productIds.Contains(s.ProductId)
                            && s.Location == request.Location
                            && !s.IsFaulty
                            && s.Quantity > 0)
                .OrderBy(s => s.Id)
                .ToListAsync(cancellationToken);

            // 4. Build sale header
            var sale = new Sale
            {
                InvoiceNo = await GenerateSequentialInvoiceNoAsync(cancellationToken),
                CustomerId = request.CustomerId,
                IsInstallment = request.IsInstallment,
                InstallmentCount = request.IsInstallment ? request.InstallmentCount : 0,
                GuarantorName = request.IsInstallment ? request.Guarantor?.Name.Trim() : null,
                GuarantorPhone = request.IsInstallment ? request.Guarantor?.Phone.Trim() : null,
                GuarantorAddress = request.IsInstallment ? request.Guarantor?.Address.Trim() : null,
                GuarantorNid = request.IsInstallment ? request.Guarantor?.NidNumber.Trim() : null
            };

            foreach (var line in lines)
            {
                var product = products[line.ProductId];
                int? linkedStockItemId = null;
                string? capturedSerial = line.SerialNumber;

                if (!string.IsNullOrWhiteSpace(line.SerialNumber))
                {
                    var serialRow = stockRows.FirstOrDefault(
                        s => s.ProductId == line.ProductId
                             && string.Equals(s.SerialNumber, line.SerialNumber, StringComparison.OrdinalIgnoreCase));

                    var availableSerial = serialRow?.Quantity ?? 0;
                    if (serialRow is null || availableSerial < line.Quantity)
                        throw new InsufficientStockException(
                            product.Id, $"{product.Name} (Serial {line.SerialNumber})", line.Quantity, availableSerial);

                    serialRow.Quantity -= line.Quantity;
                    linkedStockItemId = serialRow.Id;
                    capturedSerial = serialRow.SerialNumber;
                }
                else if (line.StockItemId.HasValue)
                {
                    var specificRow = stockRows.FirstOrDefault(
                        s => s.Id == line.StockItemId.Value && s.ProductId == line.ProductId);

                    var availableSpecific = specificRow?.Quantity ?? 0;
                    if (specificRow is null || availableSpecific < line.Quantity)
                        throw new InsufficientStockException(product.Id, product.Name, line.Quantity, availableSpecific);

                    specificRow.Quantity -= line.Quantity;
                    linkedStockItemId = specificRow.Id;
                    capturedSerial = specificRow.SerialNumber;
                }
                else
                {
                    // Pooled (non-serialized) stock rows first; if single unit and only serialized row exists, capture its serial
                    var pooledRows = stockRows
                        .Where(s => s.ProductId == line.ProductId && s.SerialNumber == null && s.Quantity > 0)
                        .ToList();

                    var rowsToUse = pooledRows.Sum(r => r.Quantity) >= line.Quantity
                        ? pooledRows
                        : stockRows.Where(s => s.ProductId == line.ProductId && s.Quantity > 0).ToList();

                    var available = rowsToUse.Sum(r => r.Quantity);
                    if (available < line.Quantity)
                        throw new InsufficientStockException(product.Id, product.Name, line.Quantity, available);

                    linkedStockItemId = rowsToUse[0].Id;
                    if (line.Quantity == 1 && rowsToUse[0].SerialNumber is not null)
                        capturedSerial = rowsToUse[0].SerialNumber;

                    var remaining = line.Quantity;
                    foreach (var row in rowsToUse)
                    {
                        if (remaining == 0) break;
                        var take = Math.Min(row.Quantity, remaining);
                        row.Quantity -= take;
                        remaining -= take;
                    }
                }

                var costAtSale = product.BuyingPrice;
                if (costAtSale < 0)
                    throw new BusinessRuleException($"Product '{product.Name}' has an invalid buying cost.");

                sale.Items.Add(new SaleItem
                {
                    ProductId = product.Id,
                    StockItemId = linkedStockItemId,
                    SerialNumber = capturedSerial,
                    Quantity = line.Quantity,
                    UnitPrice = product.SellingPrice,
                    CostPrice = costAtSale
                });
            }

            var gross = sale.Items.Sum(i => i.UnitPrice * i.Quantity);
            if (request.DiscountAmount > gross)
                throw new BusinessRuleException("Discount cannot exceed the sale amount.");

            var serviceCharge = request.IsInstallment ? request.ServiceCharge : 0m;
            if (serviceCharge < 0)
                throw new BusinessRuleException("Service charge cannot be negative.");

            sale.DiscountAmount = request.DiscountAmount;
            sale.ServiceCharge = serviceCharge;
            sale.TotalAmount = gross - request.DiscountAmount + serviceCharge;
            sale.DownPayment = request.PaidAmount;
            sale.PaidAmount = request.PaidAmount;

            // 5. Payment rules
            if (request.PaidAmount > sale.TotalAmount)
                throw new BusinessRuleException("Paid amount cannot exceed the sale total.");

            if (!request.IsInstallment && request.PaidAmount != sale.TotalAmount)
                throw new BusinessRuleException("A non-installment sale must be paid in full.");

            if (request.IsInstallment && request.PaidAmount >= sale.TotalAmount)
                throw new BusinessRuleException("An installment sale must have an outstanding balance.");

            // 6. Generate installments (3, 6, 9, or 12 months)
            if (request.IsInstallment)
            {
                var balance = sale.TotalAmount - sale.PaidAmount;
                var count = request.InstallmentCount;
                var baseAmount = decimal.Round(balance / count, 2, MidpointRounding.ToZero);
                var firstDue = (request.FirstDueDate ?? ShopTime.Today.AddMonths(1)).Date;

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
                var normalizedMethod = PaymentMethods.Normalize(request.PaymentMethod);
                sale.Payments.Add(new Payment
                {
                    Amount = sale.PaidAmount,
                    Method = normalizedMethod,
                    MobileBankingProvider = normalizedMethod == PaymentMethods.MobileBanking
                        ? PaymentMethods.NormalizeMobileProvider(request.MobileBankingProvider)
                        : null,
                    TransactionId = string.IsNullOrWhiteSpace(request.TransactionId)
                        ? null
                        : request.TransactionId.Trim(),
                    ReceiptNo = ReceiptNumber.Generate(),
                    Date = DateTime.UtcNow
                });
            }

            // 8. Save everything in one transaction
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync(cancellationToken);

            return sale.Id;
        }

        private async Task<string> GenerateSequentialInvoiceNoAsync(CancellationToken ct)
        {
            var lastId = await _context.Sales
                .AsNoTracking()
                .OrderByDescending(s => s.Id)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync(ct) ?? 0;

            var nextNumber = lastId + 1;
            while (true)
            {
                var candidate = $"INV-{nextNumber:D6}";
                var exists = await _context.Sales
                    .AsNoTracking()
                    .AnyAsync(s => s.InvoiceNo == candidate, ct);

                if (!exists)
                    return candidate;

                nextNumber++;
            }
        }
    }
}