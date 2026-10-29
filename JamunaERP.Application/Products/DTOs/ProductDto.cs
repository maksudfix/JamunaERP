using System.Text.Json.Serialization;

namespace JamunaERP.Application.Products.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ModelNo { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    // Only filled for Admin / Manager. Hidden from the JSON for everyone else.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? BuyingPrice { get; set; }

    public decimal SellingPrice { get; set; }
    public int WarrantyMonths { get; set; }
    public bool IsActive { get; set; }
}