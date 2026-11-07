using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class Sale : BaseEntity
{
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public decimal TotalAmount { get; set; }      // net sale value including ServiceCharge and minus DiscountAmount
    public decimal DownPayment { get; set; }      // initial payment collected at the time of sale
    public decimal PaidAmount { get; set; }       // total cash collected so far (down payment + installments)
    public bool IsInstallment { get; set; }
    public int InstallmentCount { get; set; }     // 3, 6, 9, or 12 months
    public decimal ServiceCharge { get; set; }    // optional installment service charge

    // Guarantor details (required for installment sales)
    public string? GuarantorName { get; set; }
    public string? GuarantorPhone { get; set; }
    public string? GuarantorAddress { get; set; }
    public string? GuarantorNid { get; set; }

    public decimal ReturnedAmount { get; set; }   // value of goods the customer returned
    public decimal RefundedAmount { get; set; }   // cash actually given back to the customer
    public decimal DiscountAmount { get; set; }   // discount given at sale time

    // Optimistic concurrency token
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<InstallmentPlan> Installments { get; set; } = new List<InstallmentPlan>();
    public ICollection<SaleReturn> Returns { get; set; } = new List<SaleReturn>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}