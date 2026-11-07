namespace JamunaERP.Application.Payments.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? MobileBankingProvider { get; set; }
    public string? TransactionId { get; set; }
    public string? ReceiptNo { get; set; }
}