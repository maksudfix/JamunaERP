using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.InstallmentPlans.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.InstallmentPlans.Queries.GetInstallmentsBySale
{
    public record GetInstallmentsBySaleQuery(int SaleId) : IRequest<List<InstallmentPlanDto>>;

    public class GetInstallmentsBySaleQueryHandler : IRequestHandler<GetInstallmentsBySaleQuery, List<InstallmentPlanDto>>
    {
        private readonly IAppDbContext _context;

        public GetInstallmentsBySaleQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<InstallmentPlanDto>> Handle(GetInstallmentsBySaleQuery request, CancellationToken cancellationToken)
        {
            return await _context.InstallmentPlans
                .AsNoTracking()
                .Where(p => p.SaleId == request.SaleId)
                .OrderBy(p => p.InstallmentNumber)
                .Select(p => new InstallmentPlanDto
                {
                    Id = p.Id,
                    SaleId = p.SaleId,
                    InstallmentNumber = p.InstallmentNumber,
                    AmountDue = p.AmountDue,
                    AmountPaid = p.AmountPaid,
                    DueDate = p.DueDate,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync(cancellationToken);
        }
    }
}