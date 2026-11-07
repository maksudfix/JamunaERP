using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class PurchaseOrderItem : BaseEntity
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int QuantityArrived { get; set; }
    public int FaultyQuantityReceived { get; set; }

    public decimal UnitCost { get; set; } // Added for tracking purchase cost
}