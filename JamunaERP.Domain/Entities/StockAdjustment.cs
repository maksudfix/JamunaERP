using JamunaERP.Domain.Common;
using JamunaERP.Domain.Enums;

namespace JamunaERP.Domain.Entities;

public class StockAdjustment : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public StockLocationType Location { get; set; }
    public bool IsFaulty { get; set; }
    public int QuantityChange { get; set; }   // + found, - lost/damaged
    public string Reason { get; set; } = string.Empty;
}