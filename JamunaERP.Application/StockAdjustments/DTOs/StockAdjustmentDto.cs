namespace JamunaERP.Application.StockAdjustments.DTOs;

public class StockAdjustmentDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsFaulty { get; set; }
    public int QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}