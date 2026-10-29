using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.WarrantyServices.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.WarrantyServices.Queries.GetWarrantyServicesByCustomer;

public record GetWarrantyServicesByCustomerQuery(int CustomerId) : IRequest<List<WarrantyServiceDto>>;

public class GetWarrantyServicesByCustomerQueryHandler : IRequestHandler<GetWarrantyServicesByCustomerQuery, List<WarrantyServiceDto>>
{
    private readonly IAppDbContext _context;

    public GetWarrantyServicesByCustomerQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WarrantyServiceDto>> Handle(GetWarrantyServicesByCustomerQuery request, CancellationToken cancellationToken)
    {
        return await _context.WarrantyServices
            .AsNoTracking()
            .Where(w => w.SaleItem.Sale.CustomerId == request.CustomerId)
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