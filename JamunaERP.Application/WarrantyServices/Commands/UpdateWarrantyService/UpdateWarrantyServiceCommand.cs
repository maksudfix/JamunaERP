using FluentValidation;
using JamunaERP.Domain.Enums;
using MediatR;

namespace JamunaERP.Application.WarrantyServices.Commands.UpdateWarrantyService
{
    public record UpdateWarrantyServiceCommand(
        int Id,
        WarrantyStatus Status,
        bool SentBackToDealerForRepair,
        string? RequiredParts
    ) : IRequest<bool>;

    public class UpdateWarrantyServiceCommandValidator : AbstractValidator<UpdateWarrantyServiceCommand>
    {
        public UpdateWarrantyServiceCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Status).IsInEnum();
            RuleFor(x => x.RequiredParts).MaximumLength(300);
        }
    }
}