using FluentValidation;

namespace JamunaERP.Application.InstallmentPlans.Commands.PayInstallment
{
    public class PayInstallmentCommandValidator : AbstractValidator<PayInstallmentCommand>
    {
        public PayInstallmentCommandValidator()
        {
            RuleFor(x => x.InstallmentPlanId).GreaterThan(0);
            RuleFor(x => x.Method).NotEmpty().MaximumLength(30);

            RuleFor(x => x.AmountPaid)
                .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        }
    }
}