using JamunaERP.Domain.Common;
using JamunaERP.Domain.Enums;

namespace JamunaERP.Domain.Entities;

public class EmployeePayment : BaseEntity
{
    // Store the Identity User ID as a string (since Identity uses string IDs)
    public string EmployeeId { get; set; } = string.Empty;
    public string ManagerId { get; set; } = string.Empty;

    public decimal BaseSalary { get; set; }
    public decimal Bonus { get; set; } = 0;
    public decimal Deductions { get; set; } = 0;
    public decimal NetPaidAmount { get; set; }

    // Period information for monthly payroll tracking
    public int PaymentMonth { get; set; } // e.g., 1 to 12
    public int PaymentYear { get; set; }  // e.g., 2026

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;

    public string? Remarks { get; set; } // e.g., "Monthly Salary + Festival Bonus"
}