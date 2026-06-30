using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Eventbox.Shared.Auditing;

public sealed class AuditSaveChangesInterceptor(
    ILogger<AuditSaveChangesInterceptor> logger,
    IAuditContextAccessor auditContextAccessor,
    TimeProvider timeProvider)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        WriteAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        WriteAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void WriteAuditEntries(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var auditContext = auditContextAccessor.GetCurrent();
        var utcNow = timeProvider.GetUtcNow();
        foreach (var entry in context.ChangeTracker.Entries().Where(ShouldAudit))
        {
            var auditEntry = CreateAuditEntry(entry, context.GetType().Name, utcNow);
            if (auditEntry is null)
            {
                continue;
            }

            using (logger.BeginScope(new Dictionary<string, object?>
            {
                ["OrganizationId"] = auditContext.OrganizationId,
                ["UserId"] = auditContext.UserId,
                ["TraceId"] = auditContext.TraceId,
                ["AuditKind"] = "EntityChange",
                ["DbContext"] = auditEntry.DbContext,
                ["EntityName"] = auditEntry.EntityName,
                ["EntityState"] = auditEntry.State
            }))
            {
                logger.LogInformation(
                    "Business audit entity {EntityName} {EntityState}: {AuditEntry}",
                    auditEntry.EntityName,
                    auditEntry.State,
                    AuditSerialization.Serialize(auditEntry));
            }
        }
    }

    private static bool ShouldAudit(EntityEntry entry)
        => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
            && !entry.Metadata.IsOwned();

    private static EntityAuditEntry? CreateAuditEntry(EntityEntry entry, string dbContextName, DateTimeOffset utcNow)
    {
        var keyValues = entry.Properties
            .Where(property => property.Metadata.IsPrimaryKey())
            .ToDictionary(
                property => property.Metadata.Name,
                property => property.CurrentValue ?? property.OriginalValue);

        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
            {
                continue;
            }

            var propertyName = property.Metadata.Name;
            switch (entry.State)
            {
                case EntityState.Added:
                    newValues[propertyName] = AuditRedactor.RedactProperty(propertyName, property.CurrentValue);
                    break;
                case EntityState.Deleted:
                    oldValues[propertyName] = AuditRedactor.RedactProperty(propertyName, property.OriginalValue);
                    break;
                case EntityState.Modified when property.IsModified:
                    oldValues[propertyName] = AuditRedactor.RedactProperty(propertyName, property.OriginalValue);
                    newValues[propertyName] = AuditRedactor.RedactProperty(propertyName, property.CurrentValue);
                    break;
            }
        }

        if (entry.State == EntityState.Modified && newValues.Count == 0)
        {
            return null;
        }

        return new EntityAuditEntry(
            dbContextName,
            entry.Metadata.ClrType.Name,
            entry.State.ToString(),
            keyValues,
            oldValues,
            newValues,
            utcNow);
    }

    private sealed record EntityAuditEntry(
        string DbContext,
        string EntityName,
        string State,
        IReadOnlyDictionary<string, object?> KeyValues,
        IReadOnlyDictionary<string, object?> OldValues,
        IReadOnlyDictionary<string, object?> NewValues,
        DateTimeOffset OccurredAt);
}
