using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Attributes;
using Dima.ChangeAudit.Data;
using Dima.ChangeAudit.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

namespace Dima.ChangeAudit.Interceptors;

public class AuditLogInterceptor(
    IServiceProvider serviceProvider,
    IUserContext userContext) : SaveChangesInterceptor
{
    // Caching attribute reflection for high-performance execution
    private static readonly ConcurrentDictionary<Type, (bool IsAuditable, string EntityName)> EntityTypeCache = new();
    private static readonly ConcurrentDictionary<PropertyInfo, bool> PropertyIgnoreCache = new();

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var mainContext = eventData.Context;

        // Prevent infinite recursive auditing if the context is ChangeAuditDbContext itself
        if (mainContext is ChangeAuditDbContext)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = BuildAuditEntries(mainContext);

        if (auditEntries.Count > 0)
        {
            // Resolve ChangeAuditDbContext from DI scope
            using var scope = serviceProvider.CreateScope();
            var auditContext = scope.ServiceProvider.GetRequiredService<ChangeAuditDbContext>();

            // Share the DB connection with main DbContext
            var dbConnection = mainContext.Database.GetDbConnection();
            auditContext.Database.SetDbConnection(dbConnection);

            // Share the active transaction if one exists
            var currentTransaction = mainContext.Database.CurrentTransaction;
            if (currentTransaction != null)
            {
                await auditContext.Database.UseTransactionAsync(
                    currentTransaction.GetDbTransaction(), cancellationToken);
            }

            // Save audit change logs atomically within the shared transaction
            auditContext.AuditChangeLogs.AddRange(auditEntries);
            await auditContext.SaveChangesAsync(cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditChangeLog> BuildAuditEntries(DbContext context)
    {
        var auditEntries = new List<AuditChangeLog>();

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditChangeLog && e.Entity is not AuditLogDetail)
            .ToList();

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType();

            // 1. Determine if entity is auditable (supports direct attributes or [MetadataType] for DB-First)
            var (isAuditable, entityName) = EntityTypeCache.GetOrAdd(entityType, ResolveAuditMetadata);
            if (!isAuditable) continue;

            // 2. Resolve Primary Key value
            var entityId = ResolvePrimaryKeyValue(entry);

            var changeLog = new AuditChangeLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityName,
                EntityId = entityId,
                Action = entry.State.ToString(),
                UserId = Guid.Parse(userContext.UserId),
                TimestampUtc = DateTime.UtcNow
            };

            // 3. Inspect Property Diffs
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;

                // Skip unmodified properties on updates
                if (entry.State == EntityState.Modified && !property.IsModified)
                    continue;

                // Check [IgnoreAudit] attribute
                var propInfo = property.Metadata.PropertyInfo;
                if (propInfo != null)
                {
                    var isIgnored = PropertyIgnoreCache.GetOrAdd(propInfo, p =>
                        p.GetCustomAttribute<IgnoreAuditAttribute>(inherit: true) != null);

                    if (isIgnored) continue;
                }

                var propertyName = property.Metadata.Name;
                var oldVal = entry.State == EntityState.Added ? null : property.OriginalValue;
                var newVal = entry.State == EntityState.Deleted ? null : property.CurrentValue;

                changeLog.Details.Add(new AuditLogDetail
                {
                    Id = Guid.NewGuid(),
                    AuditChangeLogId = changeLog.Id,
                    PropertyName = propertyName,
                    OldValue = FormatPropertyValue(oldVal),
                    NewValue = FormatPropertyValue(newVal)
                });
            }

            if (changeLog.Details.Count > 0 || entry.State == EntityState.Deleted)
            {
                auditEntries.Add(changeLog);
            }
        }

        return auditEntries;
    }

    private static (bool IsAuditable, string EntityName) ResolveAuditMetadata(Type entityType)
    {
        // Direct Attribute check
        var directAttr = entityType.GetCustomAttribute<AuditableAttribute>(inherit: true);
        if (directAttr != null)
            return (true, directAttr.EntityTypeName ?? entityType.Name);

        // Check via [MetadataType] (Supports DB-First partial classes)
        var metadataTypeAttr = entityType.GetCustomAttribute<MetadataTypeAttribute>(inherit: true);
        if (metadataTypeAttr != null)
        {
            var metaAttr = metadataTypeAttr.MetadataClassType.GetCustomAttribute<AuditableAttribute>(inherit: true);
            if (metaAttr != null)
                return (true, metaAttr.EntityTypeName ?? entityType.Name);
        }

        return (false, entityType.Name);
    }

    private static string ResolvePrimaryKeyValue(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var pkProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return pkProperty?.CurrentValue?.ToString() ?? "0";
    }

    private static string? FormatPropertyValue(object? value)
    {
        if (value is null) return null;
        if (value is string str) return str;

        return JsonSerializer.Serialize(value);
    }
}