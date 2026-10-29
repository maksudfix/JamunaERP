using FluentValidation;
using JamunaERP.Application.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(100);

        RuleFor(v => v.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(30);

        RuleFor(v => v.Address).MaximumLength(300);
    }
}