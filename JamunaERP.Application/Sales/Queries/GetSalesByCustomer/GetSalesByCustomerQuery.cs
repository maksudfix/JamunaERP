using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Sales.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Sales.Queries.GetSalesByCustomer
{
    public record GetSalesByCustomerQuery(int CustomerId) : IRequest<List<SaleDto>>;

    public class GetSalesByCustomerQueryHandler : IRequestHandler<GetSalesByCustomerQuery, List<SaleDto>>
    {
        private readonly IAppDbContext _context;

        public GetSalesByCustomerQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SaleDto>> Handle(GetSalesByCustomerQuery request, CancellationToken cancellationToken)
        {
            return await _context.Sales
                .AsNoTracking()
                .Where(s => s.CustomerId == request.CustomerId)
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
                    NetAmount = s.TotalAmount - s.ReturnedAmount,
                    IsInstallment = s.IsInstallment,
                    DiscountAmount = s.DiscountAmount,
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