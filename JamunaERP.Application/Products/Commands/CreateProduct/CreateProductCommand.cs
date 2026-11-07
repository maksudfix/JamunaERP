using MediatR;

namespace JamunaERP.Application.Products.Commands.CreateProduct
{
    public record CreateProductCommand(
        string Name,
        string Category,
        decimal BuyingPrice,
        decimal SellingPrice,
        string ModelNumber,
        int WarrantyMonths = 0,
        string Brand = "Jamuna",
        string? Barcode = null,
        int ReorderLevel = 5,
        int? DefaultDealerId = null,
        bool AllowInstallment = true
    ) : IRequest<int>;
}