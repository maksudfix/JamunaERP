using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string DealerInvoiceNo { get; set; } = string.Empty;
    public int DealerId { get; set; }
    public Dealer Dealer { get; set; } = null!;
    public DateTime ArrivalDate { get; set; }
    public DateTime? NextLotEstimatedDate { get; set; }

    public bool IsReceived { get; set; }
    public DateTime? ReceivedAt { get; set; }

    // Optimistic concurrency token (stops two people receiving the same order at once)
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}