namespace JamunaERP.Application.AuditLogs.DTOs;

public class AuditLogDto
{
    public long Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}