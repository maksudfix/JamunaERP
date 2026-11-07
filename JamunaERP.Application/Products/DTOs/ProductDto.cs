using System.Text.Json.Serialization;

namespace JamunaERP.Application.Products.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = "Jamuna";
    public string ModelNo { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Barcode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? BuyingPrice { get; set; }

    public decimal SellingPrice { get; set; }
    public int WarrantyMonths { get; set; }
    public int ReorderLevel { get; set; }
    public int? DefaultDealerId { get; set; }
    public string? DefaultDealerName { get; set; }
    public bool AllowInstallment { get; set; } = true;
    public bool IsActive { get; set; }
}