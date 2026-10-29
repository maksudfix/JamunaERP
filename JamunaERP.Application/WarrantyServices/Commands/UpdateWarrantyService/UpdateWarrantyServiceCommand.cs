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
}