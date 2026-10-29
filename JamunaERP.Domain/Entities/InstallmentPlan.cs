using JamunaERP.Domain.Common;
using JamunaERP.Domain.Enums;

namespace JamunaERP.Domain.Entities;

public class InstallmentPlan : BaseEntity
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int InstallmentNumber { get; set; } // 1 through 6
    public decimal AmountDue { get; set; }
    public DateTime DueDate { get; set; }
    public InstallmentStatus Status { get; set; } = InstallmentStatus.Pending;
    public decimal AmountPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
}