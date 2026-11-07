using FluentValidation;

namespace JamunaERP.Application.DealerPayments.Commands.RecordDealerPayment
{
    public class RecordDealerPaymentCommandValidator : AbstractValidator<RecordDealerPaymentCommand>
    {
        public RecordDealerPaymentCommandValidator()
        {
            RuleFor(x => x.DealerId).GreaterThan(0);

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Payment amount must be greater than zero.")
                .PrecisionScale(18, 2, ignoreTrailingZeros: true)
                .WithMessage("Payment amount cannot have more than 2 decimal places.");

            RuleFor(x => x.Date)
                .Must(d => d is null || d.Value <= DateTime.UtcNow.AddDays(1))
                .WithMessage("Payment date cannot be in the future.");

            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }
}