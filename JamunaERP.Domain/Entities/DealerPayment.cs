using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class DealerPayment : BaseEntity
{
    public int DealerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public Dealer Dealer { get; set; } = null!;
}