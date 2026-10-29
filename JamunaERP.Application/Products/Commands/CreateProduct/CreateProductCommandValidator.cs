using FluentValidation;

namespace JamunaERP.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ModelNumber).NotEmpty().MaximumLength(100);

            RuleFor(x => x.BuyingPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.SellingPrice).GreaterThan(0);
        }
    }
}