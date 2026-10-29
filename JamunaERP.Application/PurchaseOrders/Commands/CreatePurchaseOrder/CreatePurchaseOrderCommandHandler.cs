using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace JamunaERP.Application.PurchaseOrders.Commands.CreatePurchaseOrder
{
    public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, int>
    {
        private readonly IAppDbContext _context;

        public CreatePurchaseOrderCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            // 1. Dealer must exist and be active
            var dealer = await _context.Dealers
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == request.DealerId, cancellationToken)
                ?? throw new NotFoundException(nameof(Dealer), request.DealerId);

            if (!dealer.IsActive)
                throw new BusinessRuleException("This dealer is deactivated and cannot receive new purchase orders.");

            // 2. Friendly message instead of a raw unique-index error
            var invoiceNo = request.DealerInvoiceNo.Trim();

            var duplicate = await _context.PurchaseOrders
                .AnyAsync(p => p.DealerId == request.DealerId && p.DealerInvoiceNo == invoiceNo,
                          cancellationToken);

            if (duplicate)
                throw new BusinessRuleException(
                    $"Invoice '{invoiceNo}' already exists for dealer '{dealer.Name}'.");

            // 3. Create the order
            var purchaseOrder = new PurchaseOrder
            {
                DealerInvoiceNo = invoiceNo,
                DealerId = request.DealerId,
                ArrivalDate = request.ArrivalDate,
                NextLotEstimatedDate = request.NextLotEstimatedDate
            };

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync(cancellationToken);

            return purchaseOrder.Id;
        }
    }
}