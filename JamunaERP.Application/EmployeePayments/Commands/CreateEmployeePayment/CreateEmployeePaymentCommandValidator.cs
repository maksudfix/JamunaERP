using FluentValidation;

namespace JamunaERP.Application.EmployeePayments.Commands.CreateEmployeePayment
{
    public class CreateEmployeePaymentCommandValidator : AbstractValidator<CreateEmployeePaymentCommand>
    {
        public CreateEmployeePaymentCommandValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();

            RuleFor(x => x.BaseSalary).GreaterThan(0);
            RuleFor(x => x.Bonus).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Deductions).GreaterThanOrEqualTo(0);

            RuleFor(x => x.Deductions)
                .Must((cmd, deductions) => deductions <= cmd.BaseSalary + cmd.Bonus)
                .WithMessage("Deductions cannot exceed salary plus bonus.");

            RuleFor(x => x.PaymentMonth).InclusiveBetween(1, 12);
            RuleFor(x => x.PaymentYear).InclusiveBetween(2000, 2100);
            RuleFor(x => x.Status).IsInEnum();
            RuleFor(x => x.Remarks).MaximumLength(500);
        }
    }
}