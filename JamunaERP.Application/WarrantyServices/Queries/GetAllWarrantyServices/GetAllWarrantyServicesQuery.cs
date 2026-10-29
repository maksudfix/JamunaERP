using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.WarrantyServices.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.WarrantyServices.Queries.GetAllWarrantyServices;

public record GetAllWarrantyServicesQuery : IRequest<List<WarrantyServiceDto>>;

public class GetAllWarrantyServicesQueryHandler : IRequestHandler<GetAllWarrantyServicesQuery, List<WarrantyServiceDto>>
{
    private readonly IAppDbContext _context;

    public GetAllWarrantyServicesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WarrantyServiceDto>> Handle(GetAllWarrantyServicesQuery request, CancellationToken cancellationToken)
    {
        return await _context.WarrantyServices
            .AsNoTracking()
            .OrderByDescending(w => w.ComplaintDate)
            .Select(w => new WarrantyServiceDto
            {
                Id = w.Id,
                SaleItemId = w.SaleItemId,
                InvoiceNo = w.SaleItem.Sale.InvoiceNo,
                CustomerId = w.SaleItem.Sale.CustomerId,
                CustomerName = w.SaleItem.Sale.Customer.Name,
                ProductName = w.SaleItem.Product.Name,
                ProductSerialNumber = w.ProductSerialNumber,
                ComplaintDate = w.ComplaintDate,
                IssueDescription = w.IssueDescription,
                Status = w.Status,
                SentBackToDealerForRepair = w.SentBackToDealerForRepair,
                RequiredParts = w.RequiredParts
            })
            .ToListAsync(cancellationToken);
    }
}