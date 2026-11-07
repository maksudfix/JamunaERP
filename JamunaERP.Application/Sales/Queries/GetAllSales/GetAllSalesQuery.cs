using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Sales.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Sales.Queries.GetAllSales
{
    public record GetAllSalesQuery : IRequest<List<SaleDto>>;

    public class GetAllSalesQueryHandler : IRequestHandler<GetAllSalesQuery, List<SaleDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllSalesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SaleDto>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Sales
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SaleDto
                {
                    Id = s.Id,
                    InvoiceNo = s.InvoiceNo,
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer.Name,
                    SaleDate = s.CreatedAt,
                    TotalAmount = s.TotalAmount,
                    PaidAmount = s.PaidAmount,
                    ReturnedAmount = s.ReturnedAmount,
                    RefundedAmount = s.RefundedAmount,
                    BalanceDue = (s.TotalAmount - s.ReturnedAmount) - (s.PaidAmount - s.RefundedAmount),
                    IsInstallment = s.IsInstallment,
                    DiscountAmount = s.DiscountAmount,
                    NetAmount = s.TotalAmount - s.ReturnedAmount,
                    Items = s.Items.Select(i => new SaleItemDto
                    {
                        Id = i.Id,
                        SaleId = i.SaleId,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.Quantity * i.UnitPrice
                    }).ToList()
                })
                .ToListAsync(cancellationToken);
        }
    }
}