using JamunaERP.Domain.Common;
using JamunaERP.Domain.Enums;

namespace JamunaERP.Domain.Entities;

public class InstallmentPlan : BaseEntity
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int InstallmentNumber { get; set; }
    public decimal AmountDue { get; set; }
    public DateTime DueDate { get; set; }
    public InstallmentStatus Status { get; set; } = InstallmentStatus.Pending;
    public decimal AmountPaid { get; set; }
    public DateTime? PaymentDate { get; set; }

    // Collection follow-up tracking for overdue installments
    public string? CollectionNote { get; set; }
    public DateTime? LastFollowUpDate { get; set; }
    public DateTime? NextPromiseDate { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}