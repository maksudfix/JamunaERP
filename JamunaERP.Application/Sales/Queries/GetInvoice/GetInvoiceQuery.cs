using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Payments.DTOs;
using JamunaERP.Application.Sales.DTOs;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Sales.Queries.GetInvoice;

public record GetInvoiceQuery(int SaleId) : IRequest<InvoiceDto>;

public class GetInvoiceQueryHandler : IRequestHandler<GetInvoiceQuery, InvoiceDto>
{
    private readonly IAppDbContext _context;

    public GetInvoiceQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceQuery request, CancellationToken ct)
    {
        var sale = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .Include(s => s.Installments)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, ct)
            ?? throw new NotFoundException(nameof(Sale), request.SaleId);

        var today = ShopTime.Today;

        var lines = sale.Items
            .OrderBy(i => i.Id)
            .Select(i => new InvoiceLineDto
            {
                ProductName = i.Product.Name,
                ModelNo = i.Product.ModelNo,
                SerialNumber = i.SerialNumber,
                WarrantyMonths = i.Product.WarrantyMonths,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            })
            .ToList();

        return new InvoiceDto
        {
            SaleId = sale.Id,
            InvoiceNo = sale.InvoiceNo,
            SaleDate = sale.CreatedAt,
            CustomerName = sale.Customer.Name,
            CustomerPhone = sale.Customer.Phone,
            CustomerAlternatePhone = sale.Customer.AlternatePhone,
            CustomerNid = sale.Customer.NidNumber,
            CustomerAddress = sale.Customer.Address,
            GuarantorName = sale.GuarantorName,
            GuarantorPhone = sale.GuarantorPhone,
            GuarantorAddress = sale.GuarantorAddress,
            GuarantorNid = sale.GuarantorNid,
            Items = lines,
            SubTotal = lines.Sum(l => l.LineTotal),
            DiscountAmount = sale.DiscountAmount,
            ServiceCharge = sale.ServiceCharge,
            TotalAmount = sale.TotalAmount,
            DownPayment = sale.DownPayment,
            ReturnedAmount = sale.ReturnedAmount,
            PaidAmount = sale.PaidAmount,
            RefundedAmount = sale.RefundedAmount,
            BalanceDue = (sale.TotalAmount - sale.ReturnedAmount) - (sale.PaidAmount - sale.RefundedAmount),
            IsInstallment = sale.IsInstallment,
            Installments = sale.Installments
                .OrderBy(i => i.InstallmentNumber)
                .Select(i =>
                {
                    var effectiveStatus = i.Status == InstallmentStatus.Pending
                                          && i.DueDate.Date < today
                                          && i.AmountDue > i.AmountPaid
                        ? InstallmentStatus.Overdue
                        : i.Status;

                    return new InvoiceInstallmentDto
                    {
                        Number = i.InstallmentNumber,
                        DueDate = i.DueDate,
                        AmountDue = i.AmountDue,
                        AmountPaid = i.AmountPaid,
                        Status = effectiveStatus.ToString()
                    };
                })
                .ToList(),
            Payments = sale.Payments
                .OrderBy(p => p.Date)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    SaleId = p.SaleId,
                    Amount = p.Amount,
                    Date = p.Date,
                    Method = p.Method,
                    MobileBankingProvider = p.MobileBankingProvider,
                    TransactionId = p.TransactionId,
                    ReceiptNo = p.ReceiptNo
                })
                .ToList()
        };
    }
}