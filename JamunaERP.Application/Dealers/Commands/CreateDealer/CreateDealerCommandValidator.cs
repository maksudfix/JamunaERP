using FluentValidation;

namespace JamunaERP.Application.Dealers.Commands.CreateDealer
{
    public class CreateDealerCommandValidator : AbstractValidator<CreateDealerCommand>
    {
        public CreateDealerCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Dealer name is required.")
                .MaximumLength(150).WithMessage("Dealer name must not exceed 150 characters.");

            RuleFor(v => v.ContactPerson).MaximumLength(100);

            RuleFor(v => v.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(30).WithMessage("Phone number must not exceed 30 characters.");

            RuleFor(v => v.Address).MaximumLength(300);
        }
    }
}