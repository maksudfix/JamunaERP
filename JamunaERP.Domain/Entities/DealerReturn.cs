using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class DealerReturn : BaseEntity
{
    public int DealerId { get; set; }
    public Dealer Dealer { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }               // faulty units sent back
    public decimal CreditAmount { get; set; }       // value the dealer credits us
    public string Reason { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
}