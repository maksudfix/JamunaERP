using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Products.Queries.GetProducts
{
    public record GetProductsQuery(bool IncludeCost = false, bool IncludeInactive = false)
        : IRequest<List<ProductDto>>;

    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
    {
        private readonly IAppDbContext _context;

        public GetProductsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => request.IncludeInactive || p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Brand = p.Brand,
                    ModelNo = p.ModelNo,
                    Category = p.Category,
                    Barcode = p.Barcode,
                    BuyingPrice = p.BuyingPrice,
                    SellingPrice = p.SellingPrice,
                    WarrantyMonths = p.WarrantyMonths,
                    ReorderLevel = p.ReorderLevel,
                    DefaultDealerId = p.DefaultDealerId,
                    DefaultDealerName = p.DefaultDealer != null ? p.DefaultDealer.Name : null,
                    AllowInstallment = p.AllowInstallment,
                    IsActive = p.IsActive
                })
                .ToListAsync(cancellationToken);

            if (!request.IncludeCost)
            {
                foreach (var product in products)
                    product.BuyingPrice = null;
            }

            return products;
        }
    }
}