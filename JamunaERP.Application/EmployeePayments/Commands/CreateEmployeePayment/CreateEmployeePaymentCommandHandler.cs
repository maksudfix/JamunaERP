using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Contracts;
using JamunaERP.Domain.Entities;
using MediatR;

namespace JamunaERP.Application.EmployeePayments.Commands.CreateEmployeePayment
{
    public class CreateEmployeePaymentCommandHandler : IRequestHandler<CreateEmployeePaymentCommand, int>
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CreateEmployeePaymentCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(CreateEmployeePaymentCommand request, CancellationToken cancellationToken)
        {
            var managerId = _currentUser.UserId
                ?? throw new BusinessRuleException("Could not determine the logged-in user.");

            var payment = new EmployeePayment
            {
                EmployeeId = request.EmployeeId,
                ManagerId = managerId,                       // from the token, not the request
                BaseSalary = request.BaseSalary,
                Bonus = request.Bonus,
                Deductions = request.Deductions,
                NetPaidAmount = request.BaseSalary + request.Bonus - request.Deductions,
                PaymentMonth = request.PaymentMonth,
                PaymentYear = request.PaymentYear,
                PaymentDate = DateTime.UtcNow,
                Status = request.Status,
                Remarks = request.Remarks
            };

            _context.EmployeePayments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);
            return payment.Id;
        }
    }
}