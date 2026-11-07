using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.InstallmentPlans.DTOs;
using JamunaERP.Domain.Enums;
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
            var today = ShopTime.Today;

            var plans = await _context.InstallmentPlans
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
                    PaymentDate = p.PaymentDate,
                    CollectionNote = p.CollectionNote,
                    LastFollowUpDate = p.LastFollowUpDate,
                    NextPromiseDate = p.NextPromiseDate
                })
                .ToListAsync(cancellationToken);

            foreach (var p in plans)
            {
                if (p.Status == InstallmentStatus.Pending && p.DueDate.Date < today && p.AmountDue > p.AmountPaid)
                    p.Status = InstallmentStatus.Overdue;
            }

            return plans;
        }
    }
}