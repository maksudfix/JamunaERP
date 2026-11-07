using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
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
                Status = request.SentBackToDealerForRepair
                    ? WarrantyStatus.SentToDealer
                    : WarrantyStatus.Claimed,
                SentBackToDealerForRepair = request.SentBackToDealerForRepair,
                RequiredParts = request.RequiredParts?.Trim()
            };

            _context.WarrantyServices.Add(warrantyService);
            await _context.SaveChangesAsync(cancellationToken);

            return warrantyService.Id;
        }
    }
}