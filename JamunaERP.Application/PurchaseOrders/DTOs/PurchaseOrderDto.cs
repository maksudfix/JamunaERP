namespace JamunaERP.Application.PurchaseOrders.DTOs;

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string DealerInvoiceNo { get; set; } = string.Empty;
    public int DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public DateTime ArrivalDate { get; set; }
    public DateTime? NextLotEstimatedDate { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityArrived { get; set; }
    public int FaultyQuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }   // UnitCost x QuantityArrived
}