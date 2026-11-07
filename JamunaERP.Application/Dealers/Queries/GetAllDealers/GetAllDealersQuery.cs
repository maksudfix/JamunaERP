using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Dealers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Dealers.Queries.GetAllDealers;

public record GetAllDealersQuery(bool IncludeInactive = false) : IRequest<List<DealerDto>>;

public class GetAllDealersQueryHandler : IRequestHandler<GetAllDealersQuery, List<DealerDto>>
{
    private readonly IAppDbContext _context;

    public GetAllDealersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DealerDto>> Handle(GetAllDealersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Dealers
            .AsNoTracking()
            .Where(d => request.IncludeInactive || d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new DealerDto
            {
                Id = d.Id,
                Name = d.Name,
                ContactPerson = d.ContactPerson,
                Phone = d.Phone,
                Address = d.Address,
                PurchaseOrderCount = d.PurchaseOrders.Count,
                IsActive = d.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}