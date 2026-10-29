using JamunaERP.Domain.Enums;
using MediatR;

namespace JamunaERP.Application.WarrantyServices.Commands.CreateWarrantyService
{
    public record CreateWarrantyServiceCommand(
        int SaleItemId,
        string ProductSerialNumber,
        string IssueDescription,
        WarrantyStatus Status,
        bool SentBackToDealerForRepair,
        string? RequiredParts
    ) : IRequest<int>;
}