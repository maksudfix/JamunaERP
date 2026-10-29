namespace JamunaERP.Application.DealerPayments.DTOs;

public class DealerPaymentDto
{
    public int Id { get; set; }
    public int DealerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
}

public class DealerBalanceDto
{
    public int DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public decimal TotalPurchased { get; set; }   // UnitCost x QuantityArrived
    public decimal TotalReturned { get; set; }    // credit for faulty goods sent back
    public decimal TotalPaid { get; set; }
    public decimal Outstanding => TotalPurchased - TotalReturned - TotalPaid;
}