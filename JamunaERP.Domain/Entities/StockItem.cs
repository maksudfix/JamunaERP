using JamunaERP.Domain.Common;
using JamunaERP.Domain.Enums;

namespace JamunaERP.Domain.Entities;

public class StockItem : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public StockLocationType Location { get; set; }
    public int Quantity { get; set; }
    public bool IsFaulty { get; set; }
    public string? SerialNumber { get; set; }

    // Optimistic concurrency token
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}