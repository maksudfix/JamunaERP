using JamunaERP.Domain.Enums;

namespace JamunaERP.Application.SaleReturns.DTOs;

public class SaleReturnDto
{
    public int Id { get; set; }
    public string ReturnNo { get; set; } = string.Empty;
    public int SaleId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public StockLocationType Location { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal ReturnedValue { get; set; }
    public decimal OutstandingReduced { get; set; }
    public decimal RefundAmount { get; set; }
    public List<SaleReturnItemDto> Items { get; set; } = new();
}

public class SaleReturnItemDto
{
    public int SaleItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int FaultyQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}