using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class Sale : BaseEntity
{
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public decimal TotalAmount { get; set; }      // original sale value (never changes)
    public decimal PaidAmount { get; set; }       // cash collected so far (down payment + installments)
    public bool IsInstallment { get; set; }
    public int InstallmentCount { get; set; }

    public decimal ReturnedAmount { get; set; }   // value of goods the customer returned
    public decimal RefundedAmount { get; set; }   // cash actually given back to the customer
    public decimal DiscountAmount { get; set; }   // discount given at sale time; TotalAmount is already net of it

    // Optimistic concurrency token (stops two people editing the same sale at once)
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<InstallmentPlan> Installments { get; set; } = new List<InstallmentPlan>();
    public ICollection<SaleReturn> Returns { get; set; } = new List<SaleReturn>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}