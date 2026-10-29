using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class Payment : BaseEntity
{
    public int SaleId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Method { get; set; } = "Cash";
    public string? ReceiptNo { get; set; }
    public Sale Sale { get; set; } = null!;
}