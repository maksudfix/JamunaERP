using MediatR;

namespace JamunaERP.Application.Products.Commands.CreateProduct
{
    public record CreateProductCommand
    (
    string Name,
    string Category,
    decimal BuyingPrice,
    decimal SellingPrice,
    string ModelNumber,
    int WarrantyMonths = 0
    ) : IRequest<int>;
}