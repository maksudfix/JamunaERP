using JamunaERP.Application.Common.Interfaces; // Use the interface namespace
using JamunaERP.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JamunaERP.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IAppDbContext _context; // Depend on the interface, not Infrastructure

        public CreateProductCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = request.Name,
                Category = request.Category,
                BuyingPrice = request.BuyingPrice,
                SellingPrice = request.SellingPrice,
                ModelNo = request.ModelNumber, // Matches your Product entity property
                CreatedAt = System.DateTime.UtcNow,
                WarrantyMonths = request.WarrantyMonths,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}