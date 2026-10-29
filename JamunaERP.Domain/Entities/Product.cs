using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ModelNo { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal BuyingPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int WarrantyMonths { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}