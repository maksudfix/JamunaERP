using FluentValidation;

namespace JamunaERP.Application.PurchaseOrders.Commands.CreatePurchaseOrder
{
    public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
    {
        public CreatePurchaseOrderCommandValidator()
        {
            RuleFor(x => x.DealerInvoiceNo).NotEmpty().MaximumLength(50);
            RuleFor(x => x.DealerId).GreaterThan(0);
            RuleFor(x => x.ArrivalDate).NotEmpty();

            RuleFor(x => x.NextLotEstimatedDate)
                .Must((cmd, next) => next is null || next.Value.Date >= cmd.ArrivalDate.Date)
                .WithMessage("Next lot date cannot be earlier than the arrival date.");
        }
    }
}