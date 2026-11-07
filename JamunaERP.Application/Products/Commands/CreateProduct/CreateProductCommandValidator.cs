using FluentValidation;

namespace JamunaERP.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ModelNumber).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Barcode).MaximumLength(100);

            RuleFor(x => x.BuyingPrice)
                .GreaterThanOrEqualTo(0)
                .PrecisionScale(18, 2, ignoreTrailingZeros: true);

            RuleFor(x => x.SellingPrice)
                .GreaterThan(0)
                .PrecisionScale(18, 2, ignoreTrailingZeros: true);

            RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DefaultDealerId).GreaterThan(0).When(x => x.DefaultDealerId.HasValue);
        }
    }
}