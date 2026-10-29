using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.WarrantyServices.Commands.CreateWarrantyService
{
    public class CreateWarrantyServiceCommandHandler : IRequestHandler<CreateWarrantyServiceCommand, int>
    {
        private readonly IAppDbContext _context;

        public CreateWarrantyServiceCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateWarrantyServiceCommand request, CancellationToken cancellationToken)
        {
            // A claim must belong to a real sale item (clear error instead of a database FK error)
            var saleItemExists = await _context.SaleItems
                .AnyAsync(i => i.Id == request.SaleItemId, cancellationToken);
            if (!saleItemExists)
                throw new NotFoundException(nameof(SaleItem), request.SaleItemId);

            var warrantyService = new WarrantyService
            {
                SaleItemId = request.SaleItemId,
                ProductSerialNumber = request.ProductSerialNumber.Trim(),
                ComplaintDate = DateTime.UtcNow,
                IssueDescription = request.IssueDescription.Trim(),
                Status = request.Status,
                SentBackToDealerForRepair = request.SentBackToDealerForRepair,
                RequiredParts = request.RequiredParts?.Trim()
            };

            _context.WarrantyServices.Add(warrantyService);
            await _context.SaveChangesAsync(cancellationToken);

            return warrantyService.Id;
        }
    }
}