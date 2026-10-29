using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.InstallmentPlans.Commands.PayInstallment
{
    public record PayInstallmentCommand(
        int InstallmentPlanId,
        decimal AmountPaid,
        string Method = "Cash"
    ) : IRequest<bool>;

    public class PayInstallmentCommandHandler : IRequestHandler<PayInstallmentCommand, bool>
    {
        private readonly IAppDbContext _context;

        public PayInstallmentCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(PayInstallmentCommand request, CancellationToken cancellationToken)
        {
            var plan = await _context.InstallmentPlans
                .Include(p => p.Sale)
                .FirstOrDefaultAsync(p => p.Id == request.InstallmentPlanId, cancellationToken)
                ?? throw new NotFoundException(nameof(InstallmentPlan), request.InstallmentPlanId);

            if (plan.Status == InstallmentStatus.Paid)
                throw new BusinessRuleException("This installment is already fully paid.");

            var remaining = plan.AmountDue - plan.AmountPaid;
            if (request.AmountPaid > remaining)
                throw new BusinessRuleException(
                    $"Payment exceeds the remaining balance of {remaining:0.00} for this installment.");

            var now = DateTime.UtcNow;   // server time, never from the client

            plan.AmountPaid += request.AmountPaid;
            plan.PaymentDate = now;

            // Keep the sale header in sync with what has actually been collected
            plan.Sale.PaidAmount += request.AmountPaid;

            // Receipt for this payment
            _context.Payments.Add(new Payment
            {
                SaleId = plan.SaleId,
                Amount = request.AmountPaid,
                Date = now,
                Method = request.Method,
                ReceiptNo = ReceiptNumber.Generate()
            });

            if (plan.AmountPaid >= plan.AmountDue)
                plan.Status = InstallmentStatus.Paid;
            else
                plan.Status = plan.DueDate.Date < ShopTime.Today
                    ? InstallmentStatus.Overdue
                    : InstallmentStatus.Pending;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}