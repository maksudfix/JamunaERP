using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IAppDbContext _context;

        public CreateProductCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            if (request.DefaultDealerId.HasValue)
            {
                var dealerExists = await _context.Dealers
                    .AnyAsync(d => d.Id == request.DefaultDealerId.Value, cancellationToken);
                if (!dealerExists)
                    throw new NotFoundException(nameof(Dealer), request.DefaultDealerId.Value);
            }

            var product = new Product
            {
                Name = request.Name.Trim(),
                Brand = string.IsNullOrWhiteSpace(request.Brand) ? "Jamuna" : request.Brand.Trim(),
                Category = request.Category.Trim(),
                BuyingPrice = request.BuyingPrice,
                SellingPrice = request.SellingPrice,
                ModelNo = request.ModelNumber.Trim(),
                Barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim(),
                WarrantyMonths = request.WarrantyMonths,
                ReorderLevel = request.ReorderLevel,
                DefaultDealerId = request.DefaultDealerId,
                AllowInstallment = request.AllowInstallment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}