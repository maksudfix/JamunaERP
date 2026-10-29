using FluentValidation;

namespace JamunaERP.Application.Sales.Commands.CreateSale
{
    public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
    {
        public CreateSaleCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.Location).IsInEnum();
            RuleFor(x => x.PaidAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(50);

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("A sale must contain at least one item.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).GreaterThan(0);
                item.RuleFor(i => i.Quantity).GreaterThan(0);
            });

            When(x => x.IsInstallment, () =>
            {
                RuleFor(x => x.InstallmentCount).InclusiveBetween(1, 6)
                    .WithMessage("Installment count must be between 1 and 6.");

                RuleFor(x => x.FirstDueDate)
                    .Must(d => d is null || d.Value.Date >= DateTime.UtcNow.Date)
                    .WithMessage("First due date cannot be in the past.");
            });
        }
    }
}