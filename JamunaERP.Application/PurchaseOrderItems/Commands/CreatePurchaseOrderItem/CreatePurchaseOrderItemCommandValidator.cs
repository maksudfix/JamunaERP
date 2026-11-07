using FluentValidation;

namespace JamunaERP.Application.PurchaseOrderItems.Commands.CreatePurchaseOrderItem
{
    public class CreatePurchaseOrderItemCommandValidator : AbstractValidator<CreatePurchaseOrderItemCommand>
    {
        public CreatePurchaseOrderItemCommandValidator()
        {
            RuleFor(x => x.PurchaseOrderId).GreaterThan(0);
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.QuantityArrived).GreaterThan(0);
            RuleFor(x => x.UnitCost)
                .GreaterThan(0)
                .PrecisionScale(18, 2, ignoreTrailingZeros: true)
                .WithMessage("Unit cost cannot have more than 2 decimal places.");
            RuleFor(x => x.FaultyQuantityReceived).GreaterThanOrEqualTo(0);

            RuleFor(x => x.FaultyQuantityReceived)
                .LessThanOrEqualTo(x => x.QuantityArrived)
                .WithMessage("Faulty quantity cannot exceed the quantity arrived.");
        }
    }
}