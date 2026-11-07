using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class SaleItem : BaseEntity
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int? StockItemId { get; set; }
    public string? SerialNumber { get; set; } // Captured when a serialized unit is sold

    public StockItem? StockItem { get; set; }
}