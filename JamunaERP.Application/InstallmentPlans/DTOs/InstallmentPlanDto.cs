using JamunaERP.Domain.Enums;

namespace JamunaERP.Application.InstallmentPlans.DTOs;

public class InstallmentPlanDto
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime DueDate { get; set; }
    public InstallmentStatus Status { get; set; }
    public DateTime? PaymentDate { get; set; }
}