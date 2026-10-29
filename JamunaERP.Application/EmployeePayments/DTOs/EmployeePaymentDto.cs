using JamunaERP.Domain.Enums;

namespace JamunaERP.Application.EmployeePayments.DTOs;

public class EmployeePaymentDto
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string ManagerId { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal Bonus { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetPaidAmount { get; set; }
    public int PaymentMonth { get; set; }
    public int PaymentYear { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentStatus Status { get; set; }
    public string? Remarks { get; set; }
}