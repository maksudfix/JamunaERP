using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Customers.DTOs;
using JamunaERP.Application.Payments.DTOs;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Customers.Queries.GetCustomerStatement;

public record GetCustomerStatementQuery(int CustomerId) : IRequest<CustomerStatementDto>;

public class GetCustomerStatementQueryHandler : IRequestHandler<GetCustomerStatementQuery, CustomerStatementDto>
{
    private readonly IAppDbContext _context;

    public GetCustomerStatementQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerStatementDto> Handle(GetCustomerStatementQuery request, CancellationToken ct)
    {
        var customer = await _context.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        var sales = await _context.Sales.AsNoTracking()
            .Where(s => s.CustomerId == request.CustomerId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new CustomerStatementSaleDto
            {
                SaleId = s.Id,
                InvoiceNo = s.InvoiceNo,
                SaleDate = s.CreatedAt,
                TotalAmount = s.TotalAmount,
                DiscountAmount = s.DiscountAmount,
                ReturnedAmount = s.ReturnedAmount,
                PaidAmount = s.PaidAmount,
                RefundedAmount = s.RefundedAmount,
                BalanceDue = (s.TotalAmount - s.ReturnedAmount) - (s.PaidAmount - s.RefundedAmount),
                Payments = s.Payments
                    .OrderBy(p => p.Date)
                    .Select(p => new PaymentDto
                    {
                        Id = p.Id,
                        SaleId = p.SaleId,
                        Amount = p.Amount,
                        Date = p.Date,
                        Method = p.Method,
                        ReceiptNo = p.ReceiptNo
                    }).ToList()
            })
            .ToListAsync(ct);

        return new CustomerStatementDto
        {
            CustomerId = customer.Id,
            Name = customer.Name,
            Phone = customer.Phone,
            Address = customer.Address,
            TotalSales = sales.Sum(s => s.TotalAmount - s.ReturnedAmount),
            TotalPaid = sales.Sum(s => s.PaidAmount - s.RefundedAmount),
            TotalDue = sales.Sum(s => s.BalanceDue),
            Sales = sales
        };
    }
}