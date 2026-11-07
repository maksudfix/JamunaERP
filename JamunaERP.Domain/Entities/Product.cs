using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = "Jamuna";
    public string ModelNo { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal BuyingPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int WarrantyMonths { get; set; }
    public int ReorderLevel { get; set; } = 5;
    public int? DefaultDealerId { get; set; }
    public Dealer? DefaultDealer { get; set; }
    public bool AllowInstallment { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}