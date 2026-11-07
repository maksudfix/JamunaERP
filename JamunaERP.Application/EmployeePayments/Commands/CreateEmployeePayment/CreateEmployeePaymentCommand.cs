using JamunaERP.Domain.Enums;
using MediatR;

namespace JamunaERP.Application.EmployeePayments.Commands.CreateEmployeePayment
{
    public record CreateEmployeePaymentCommand(
        string EmployeeId,
        decimal BaseSalary,
        decimal Bonus,
        decimal Deductions,
        int PaymentMonth,
        int PaymentYear,
        PaymentStatus Status,
        string? Remarks
    ) : IRequest<int>;
}