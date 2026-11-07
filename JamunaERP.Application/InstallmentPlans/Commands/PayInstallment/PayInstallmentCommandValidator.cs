using FluentValidation;
using JamunaERP.Application.Common;

namespace JamunaERP.Application.InstallmentPlans.Commands.PayInstallment
{
    public class PayInstallmentCommandValidator : AbstractValidator<PayInstallmentCommand>
    {
        public PayInstallmentCommandValidator()
        {
            RuleFor(x => x.InstallmentPlanId).GreaterThan(0);

            RuleFor(x => x.Method)
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

            RuleFor(x => x.AmountPaid)
                .GreaterThan(0).WithMessage("Payment amount must be greater than zero.")
                .PrecisionScale(18, 2, ignoreTrailingZeros: true)
                .WithMessage("Payment amount cannot have more than 2 decimal places.");
        }
    }
}