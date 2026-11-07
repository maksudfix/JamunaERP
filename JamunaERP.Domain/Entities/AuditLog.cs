namespace JamunaERP.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public string EntityName { get; set; } = string.Empty; // e.g., "Product", "Expense", "Sale"
    public string EntityId { get; set; } = string.Empty;   // Primary key of the modified/deleted row
    public string Action { get; set; } = string.Empty;     // "Modified" or "Deleted"
    public string? UserId { get; set; }                    // Logged-in user who made the change
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; }                 // JSON snapshot before change
    public string? NewValues { get; set; }                 // JSON snapshot after change (null on delete)
}