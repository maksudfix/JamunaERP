using FluentValidation;
using MediatR;

namespace JamunaERP.Application.WarrantyServices.Commands.CreateWarrantyService
{
    public record CreateWarrantyServiceCommand(
        int SaleItemId,
        string ProductSerialNumber,
        string IssueDescription,
        bool SentBackToDealerForRepair = false,
        string? RequiredParts = null
    ) : IRequest<int>;

    public class CreateWarrantyServiceCommandValidator : AbstractValidator<CreateWarrantyServiceCommand>
    {
        public CreateWarrantyServiceCommandValidator()
        {
            RuleFor(x => x.SaleItemId).GreaterThan(0);
            RuleFor(x => x.ProductSerialNumber).NotEmpty().MaximumLength(100);
            RuleFor(x => x.IssueDescription).NotEmpty().MaximumLength(500);
            RuleFor(x => x.RequiredParts).MaximumLength(300);
        }
    }
}