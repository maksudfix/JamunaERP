namespace JamunaERP.Application.DealerReturns.DTOs;

public class DealerReturnDto
{
    public int Id { get; set; }
    public int DealerId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal CreditAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
}