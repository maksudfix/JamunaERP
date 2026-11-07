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
        string Method = "Cash",
        string? MobileBankingProvider = null,
        string? TransactionId = null
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

            if (plan.Status == InstallmentStatus.Cancelled)
                throw new BusinessRuleException("This installment has been cancelled.");

            var remaining = plan.AmountDue - plan.AmountPaid;
            if (request.AmountPaid > remaining)
                throw new BusinessRuleException(
                    $"Payment exceeds the remaining balance of {remaining:0.00} for this installment.");

            var now = DateTime.UtcNow;
            var normalizedMethod = PaymentMethods.Normalize(request.Method);

            plan.AmountPaid += request.AmountPaid;
            plan.PaymentDate = now;
            plan.Sale.PaidAmount += request.AmountPaid;

            _context.Payments.Add(new Payment
            {
                SaleId = plan.SaleId,
                Amount = request.AmountPaid,
                Date = now,
                Method = normalizedMethod,
                MobileBankingProvider = normalizedMethod == PaymentMethods.MobileBanking
                    ? PaymentMethods.NormalizeMobileProvider(request.MobileBankingProvider)
                    : null,
                TransactionId = string.IsNullOrWhiteSpace(request.TransactionId)
                    ? null
                    : request.TransactionId.Trim(),
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