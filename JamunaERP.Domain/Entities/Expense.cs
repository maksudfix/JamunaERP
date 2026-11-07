using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class Expense : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;   // Rent, Electricity, Transport...
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}