using JamunaERP.Application.Payments.DTOs;

namespace JamunaERP.Application.Sales.DTOs;

public class InvoiceDto
{
    public int SaleId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }              // UTC

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;

    public List<InvoiceLineDto> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ReturnedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal BalanceDue { get; set; }

    public bool IsInstallment { get; set; }
    public List<InvoiceInstallmentDto> Installments { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class InvoiceLineDto
{
    public string ProductName { get; set; } = string.Empty;
    public string ModelNo { get; set; } = string.Empty;
    public int WarrantyMonths { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class InvoiceInstallmentDto
{
    public int Number { get; set; }
    public DateTime DueDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public string Status { get; set; } = string.Empty;
}