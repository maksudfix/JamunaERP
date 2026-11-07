using FluentValidation;
using JamunaERP.Application.Common;

namespace JamunaERP.Application.Sales.Commands.CreateSale
{
    public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
    {
        public CreateSaleCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.Location).IsInEnum();

            RuleFor(x => x.PaidAmount)
                .GreaterThanOrEqualTo(0)
                .PrecisionScale(18, 2, ignoreTrailingZeros: true);

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0)
                .PrecisionScale(18, 2, ignoreTrailingZeros: true);

            RuleFor(x => x.ServiceCharge)
                .GreaterThanOrEqualTo(0)
                .PrecisionScale(18, 2, ignoreTrailingZeros: true);

            RuleFor(x => x.PaymentMethod)
                .NotEmpty()
                .MaximumLength(30)
                .Must(PaymentMethods.IsValid)
                .WithMessage($"Payment method must be one of: {string.Join(", ", PaymentMethods.All)}.");

            When(x => !string.IsNullOrWhiteSpace(x.MobileBankingProvider), () =>
            {
                RuleFor(x => x.MobileBankingProvider)
                    .Must(PaymentMethods.IsValidMobileProvider)
                    .WithMessage($"Mobile banking provider must be one of: {string.Join(", ", PaymentMethods.MobileProviders)}.");
            });

            RuleFor(x => x.TransactionId).MaximumLength(100);

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("A sale must contain at least one item.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).GreaterThan(0);
                item.RuleFor(i => i.Quantity).GreaterThan(0);
                item.RuleFor(i => i.StockItemId).GreaterThan(0).When(i => i.StockItemId.HasValue);
                item.RuleFor(i => i.SerialNumber).MaximumLength(100);
                item.RuleFor(i => i.Quantity)
                    .Equal(1)
                    .When(i => !string.IsNullOrWhiteSpace(i.SerialNumber))
                    .WithMessage("A sale line with a serial number must have a quantity of 1.");
            });

            When(x => x.IsInstallment, () =>
            {
                RuleFor(x => x.InstallmentCount)
                    .Must(c => PaymentMethods.AllowedInstallmentMonths.Contains(c))
                    .WithMessage($"Installment count must be one of: {string.Join(", ", PaymentMethods.AllowedInstallmentMonths)} months.");

                RuleFor(x => x.FirstDueDate)
                    .Must(d => d is null || d.Value.Date >= ShopTime.Today)
                    .WithMessage("First due date cannot be in the past.");

                RuleFor(x => x.Guarantor)
                    .NotNull().WithMessage("Guarantor information is required for installment sales.");

                When(x => x.Guarantor is not null, () =>
                {
                    RuleFor(x => x.Guarantor!.Name).NotEmpty().MaximumLength(100);
                    RuleFor(x => x.Guarantor!.Phone).NotEmpty().MaximumLength(30);
                    RuleFor(x => x.Guarantor!.Address).NotEmpty().MaximumLength(300);
                    RuleFor(x => x.Guarantor!.NidNumber).NotEmpty().MaximumLength(30);
                });
            });
        }
    }
}