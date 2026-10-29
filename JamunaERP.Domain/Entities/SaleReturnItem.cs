using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class SaleReturnItem : BaseEntity
{
    public int SaleReturnId { get; set; }
    public SaleReturn SaleReturn { get; set; } = null!;

    public int SaleItemId { get; set; }
    public SaleItem SaleItem { get; set; } = null!;

    public int Quantity { get; set; }          // total units returned on this line
    public int FaultyQuantity { get; set; }    // how many of them are faulty (go to the faulty pool)
    public decimal UnitPrice { get; set; }     // price snapshot from the sale
}