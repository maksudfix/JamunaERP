using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class Dealer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public ICollection<DealerPayment> Payments { get; set; } = new List<DealerPayment>();
}