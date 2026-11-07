using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class Payment : BaseEntity
{
    public int SaleId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Method { get; set; } = "Cash";           // Cash, Card, MobileBanking, Refund
    public string? MobileBankingProvider { get; set; }     // bKash, Nagad, Rocket
    public string? TransactionId { get; set; }             // Mobile banking or card TrxID
    public string? ReceiptNo { get; set; }
    public Sale Sale { get; set; } = null!;
}