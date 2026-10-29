using FluentValidation;

namespace JamunaERP.Application.StockItems.Commands.CreateStockItem
{
    public class CreateStockItemCommandValidator : AbstractValidator<CreateStockItemCommand>
    {
        public CreateStockItemCommandValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.Location).IsInEnum();
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.SerialNumber).MaximumLength(100);

            // A serial number identifies one physical unit
            When(x => !string.IsNullOrWhiteSpace(x.SerialNumber), () =>
            {
                RuleFor(x => x.Quantity).Equal(1)
                    .WithMessage("A stock row with a serial number must have a quantity of 1.");
            });
        }
    }
}