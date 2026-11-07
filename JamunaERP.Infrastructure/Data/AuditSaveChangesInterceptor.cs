using System.Text.Json;
using JamunaERP.Application.Contracts;
using JamunaERP.Domain.Common;
using JamunaERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JamunaERP.Infrastructure.Data;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> IgnoredProperties = new(StringComparer.Ordinal)
    {
        nameof(BaseEntity.CreatedAt),
        nameof(BaseEntity.CreatedBy),
        nameof(BaseEntity.UpdatedAt),
        nameof(BaseEntity.UpdatedBy),
        "RowVersion"
    };

    private readonly ICurrentUserService _currentUser;

    public AuditSaveChangesInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId;
        var auditLogs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is not BaseEntity entity) continue;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
                entity.CreatedBy = userId;

                if (userId is not null)
                {
                    if (entry.Entity is Sale)
                        entry.Property("EmployeeId").CurrentValue = userId;
                    else if (entry.Entity is PurchaseOrder)
                        entry.Property("CreatedById").CurrentValue = userId;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                // Creation info must never change
                entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;

                entity.UpdatedAt = now;
                entity.UpdatedBy = userId;

                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                foreach (var prop in entry.Properties)
                {
                    if (!prop.IsModified || prop.Metadata.IsConcurrencyToken || IgnoredProperties.Contains(prop.Metadata.Name))
                        continue;

                    if (Equals(prop.OriginalValue, prop.CurrentValue))
                        continue;

                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                }

                if (oldValues.Count > 0)
                {
                    auditLogs.Add(new AuditLog
                    {
                        EntityName = entry.Metadata.ClrType.Name,
                        EntityId = entity.Id.ToString(),
                        Action = "Modified",
                        UserId = userId,
                        TimestampUtc = now,
                        OldValues = JsonSerializer.Serialize(oldValues),
                        NewValues = JsonSerializer.Serialize(newValues)
                    });
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                var oldValues = new Dictionary<string, object?>();

                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.IsConcurrencyToken || IgnoredProperties.Contains(prop.Metadata.Name))
                        continue;

                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                }

                auditLogs.Add(new AuditLog
                {
                    EntityName = entry.Metadata.ClrType.Name,
                    EntityId = entity.Id.ToString(),
                    Action = "Deleted",
                    UserId = userId,
                    TimestampUtc = now,
                    OldValues = JsonSerializer.Serialize(oldValues),
                    NewValues = null
                });
            }
        }

        if (auditLogs.Count > 0)
            context.Set<AuditLog>().AddRange(auditLogs);
    }
}