using JamunaERP.Domain.Common;
using JamunaERP.Domain.Enums;

namespace JamunaERP.Domain.Entities;

public class SaleReturn : BaseEntity
{
    public string ReturnNo { get; set; } = string.Empty;

    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    // Where the returned goods were put back
    public StockLocationType Location { get; set; }

    public string Reason { get; set; } = string.Empty;

    public decimal ReturnedValue { get; set; }        // value of the goods
    public decimal OutstandingReduced { get; set; }   // removed from what the customer still owed
    public decimal RefundAmount { get; set; }         // cash paid back

    public ICollection<SaleReturnItem> Items { get; set; } = new List<SaleReturnItem>();
}