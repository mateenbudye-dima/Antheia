using ChangeAudit.Abstractions;
using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Attributes;
using Dima.ChangeAudit.Data;
using Dima.ChangeAudit.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    IUserContext userContext,
    IAuditHierarchyResolver hierarchyResolver) : SaveChangesInterceptor
{
    // Caching attribute reflection for high-performance execution
    private static readonly ConcurrentDictionary<
        Type,
        (bool IsAuditable, string EntityName)> EntityTypeCache = new();

    private static readonly ConcurrentDictionary<
        PropertyInfo,
        bool> PropertyIgnoreCache = new();

    private static readonly HashSet<string> AutoIgnoredProperties =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "CreatedBy",
            "CreatedDate",
            "UpdatedBy",
            "UpdatedDate"
        };

    /*
     * Holds audit information captured before SaveChanges.
     *
     * We cannot save the audit records yet because Added entities may
     * still have EF temporary identity values.
     */
    private readonly ConcurrentDictionary<
        DbContext,
        List<PendingAuditEntry>> pendingAudits = new();

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);

        var mainContext = eventData.Context;

        // Don't audit the audit database itself.
        if (mainContext is ChangeAuditDbContext)
        {
            return await base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        var auditEntries = await BuildAuditEntriesAsync(
            mainContext,
            cancellationToken);

        if (auditEntries.Count > 0)
        {
            pendingAudits[mainContext] = auditEntries;
        }

        return await base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return await base.SavedChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        var mainContext = eventData.Context;

        if (mainContext is ChangeAuditDbContext)
        {
            return await base.SavedChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        if (!pendingAudits.TryRemove(
                mainContext,
                out var auditEntries))
        {
            return await base.SavedChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        /*
         * At this point SQL Server has generated identity values and
         * EF Core has populated them back onto the Added entities.
         */
        foreach (var pendingAudit in auditEntries)
        {
            pendingAudit.AuditLog.EntityId =
                ResolvePrimaryKeyValue(pendingAudit.Entry);
        }

        await SaveAuditEntriesAsync(
            mainContext,
            auditEntries,
            cancellationToken);

        return await base.SavedChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            // Main SaveChanges failed, so don't retain pending audit entries.
            pendingAudits.TryRemove(
                eventData.Context,
                out _);
        }

        return base.SaveChangesFailedAsync(
            eventData,
            cancellationToken);
    }

    private async Task SaveAuditEntriesAsync(
        DbContext mainContext,
        List<PendingAuditEntry> auditEntries,
        CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var auditContext =
            scope.ServiceProvider.GetRequiredService<ChangeAuditDbContext>();

        /*
         * Use the same database connection as the main context.
         */
        var dbConnection = mainContext.Database.GetDbConnection();

        auditContext.Database.SetDbConnection(dbConnection);

        /*
         * If the caller supplied an explicit transaction and it is
         * still active, use that transaction.
         */
        var currentTransaction =
            mainContext.Database.CurrentTransaction;

        if (currentTransaction != null)
        {
            await auditContext.Database.UseTransactionAsync(
                currentTransaction.GetDbTransaction(),
                cancellationToken);
        }

        auditContext.AuditChangeLogs.AddRange(
            auditEntries.Select(x => x.AuditLog));

        await auditContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<List<PendingAuditEntry>> BuildAuditEntriesAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        var auditEntries = new List<PendingAuditEntry>();

        var entries = context.ChangeTracker.Entries()
            .Where(e =>
                e.State is EntityState.Added
                    or EntityState.Modified
                    or EntityState.Deleted)
            .Where(e =>
                e.Entity is not AuditChangeLog &&
                e.Entity is not AuditLogDetail)
            .ToList();

        foreach (var entry in entries)
        {
            /*
             * Use EF metadata CLR type instead of entry.Entity.GetType()
             * so proxy types don't break attribute lookup.
             */
            var entityType = entry.Metadata.ClrType;

            var (isAuditable, entityName) =
                EntityTypeCache.GetOrAdd(
                    entityType,
                    ResolveAuditMetadata);

            if (!isAuditable)
                continue;

            /*
             * IMPORTANT:
             *
             * For Added entities this may be a temporary EF value such as:
             *
             * -2147482647
             *
             * We intentionally do NOT treat this as the final ID.
             * SavedChangesAsync() resolves the real identity later.
             */
            var entityId = ResolvePrimaryKeyValue(entry);

            var hierarchy =
                await hierarchyResolver.ResolveAsync(
                    entry,
                    context,
                    cancellationToken);

            var changeLog = new AuditChangeLog
            {
                Id = Guid.NewGuid(),

                EntityType = entityName,

                /*
                 * This is only a temporary value for Added entities.
                 * It will be replaced in SavedChangesAsync().
                 */
                EntityId = entityId,

                Action = entry.State.ToString(),

                UserId = ResolveUserId(),

                TimestampUtc = DateTime.UtcNow,

                HierarchyJson = hierarchy == null
                    ? null
                    : JsonSerializer.Serialize(hierarchy)
            };

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;

                if (AutoIgnoredProperties.Contains(
                        property.Metadata.Name))
                {
                    continue;
                }

                if (entry.State == EntityState.Modified &&
                    !property.IsModified)
                {
                    continue;
                }

                var propInfo = property.Metadata.PropertyInfo;

                if (propInfo != null)
                {
                    var isIgnored =
                        PropertyIgnoreCache.GetOrAdd(
                            propInfo,
                            p =>
                                p.GetCustomAttribute<IgnoreAuditAttribute>(
                                    inherit: true) != null);

                    if (isIgnored)
                        continue;
                }

                var propertyName = property.Metadata.Name;

                var oldValue =
                    entry.State == EntityState.Added
                        ? null
                        : property.OriginalValue;

                var newValue =
                    entry.State == EntityState.Deleted
                        ? null
                        : property.CurrentValue;

                changeLog.Details.Add(
                    new AuditLogDetail
                    {
                        Id = Guid.NewGuid(),

                        AuditChangeLogId = changeLog.Id,

                        PropertyName = propertyName,

                        OldValue = FormatPropertyValue(oldValue),

                        NewValue = FormatPropertyValue(newValue)
                    });
            }

            if (changeLog.Details.Count > 0 ||
                entry.State == EntityState.Deleted)
            {
                auditEntries.Add(
                    new PendingAuditEntry
                    {
                        Entry = entry,
                        AuditLog = changeLog
                    });
            }
        }

        return auditEntries;
    }

    private static (bool IsAuditable, string EntityName)
        ResolveAuditMetadata(Type entityType)
    {
        // Direct Attribute check
        var directAttr =
            entityType.GetCustomAttribute<AuditableAttribute>(
                inherit: true);

        if (directAttr != null)
        {
            return (
                true,
                directAttr.EntityTypeName ?? entityType.Name);
        }

        // Check via [MetadataType]
        // Supports DB-First partial classes.
        var metadataTypeAttr =
            entityType.GetCustomAttribute<MetadataTypeAttribute>(
                inherit: true);

        if (metadataTypeAttr != null)
        {
            var metaAttr =
                metadataTypeAttr.MetadataClassType
                    .GetCustomAttribute<AuditableAttribute>(
                        inherit: true);

            if (metaAttr != null)
            {
                return (
                    true,
                    metaAttr.EntityTypeName ?? entityType.Name);
            }
        }

        return (
            false,
            entityType.Name);
    }

    private static string ResolvePrimaryKeyValue(
        EntityEntry entry)
    {
        var primaryKey =
            entry.Metadata.FindPrimaryKey();

        if (primaryKey == null)
            return "0";

        var values = primaryKey.Properties
            .Select(property =>
                entry.Property(property.Name)
                    .CurrentValue?
                    .ToString() ?? "0");

        return string.Join(",", values);
    }

    private Guid ResolveUserId()
    {
        if (Guid.TryParse(
                userContext.UserId,
                out var userId))
        {
            return userId;
        }

        throw new InvalidOperationException(
            "A valid user ID is required for audit logging.");
    }

    private static string? FormatPropertyValue(
        object? value)
    {
        if (value is null)
            return null;

        if (value is string str)
            return str;

        return JsonSerializer.Serialize(value);
    }

    private sealed class PendingAuditEntry
    {
        public required EntityEntry Entry { get; init; }

        public required AuditChangeLog AuditLog { get; init; }
    }
}