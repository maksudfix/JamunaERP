using JamunaERP.Application.Payments.DTOs;

namespace JamunaERP.Application.Customers.DTOs;

public class CustomerStatementDto
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }     // net of returns
    public decimal TotalPaid { get; set; }      // net of refunds
    public decimal TotalDue { get; set; }
    public List<CustomerStatementSaleDto> Sales { get; set; } = new();
}

public class CustomerStatementSaleDto
{
    public int SaleId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ReturnedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
}