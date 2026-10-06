using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Models.Domain;
using Dima.ChangeAudit.Attributes;
using Dima.ChangeAudit.Data;
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
    private static readonly ConcurrentDictionary<
        Type,
        (bool IsAuditable, string EntityName)> EntityTypeCache = new();

    // Combined metadata cache to replace separate PropertyIgnoreCache & LogOnAddedCache
    private static readonly ConcurrentDictionary<
        PropertyInfo,
        (bool IsIgnored, bool LogOnAdded)> PropertyMetadataCache = new();

    private static readonly HashSet<string> AutoIgnoredProperties =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "CreatedBy",
            "CreatedDate",
            "UpdatedBy",
            "UpdatedDate"
        };

    private readonly ConcurrentDictionary<
        DbContext,
        List<PendingAuditEntry>> _pendingAudits = new();

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null || eventData.Context is ChangeAuditDbContext)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var context = eventData.Context;

        var auditEntries = await BuildAuditEntriesAsync(context, cancellationToken);

        if (auditEntries.Count > 0)
        {
            _pendingAudits[context] = auditEntries;
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null || eventData.Context is ChangeAuditDbContext)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        var context = eventData.Context;

        try
        {
            if (_pendingAudits.TryRemove(context, out var auditEntries) && auditEntries.Count > 0)
            {
                foreach (var pendingAudit in auditEntries)
                {
                    var audit = pendingAudit.AuditLog;

                    audit.EntityId = ResolvePrimaryKeyValue(pendingAudit.Entry);

                    /*
                     * If the changed entity itself is the root,
                     * its RootEntityId must also use the real generated ID.
                     */
                    if (pendingAudit.IsSelfRoot)
                    {
                        audit.RootEntityId = audit.EntityId;
                    }
                }

                await SaveAuditEntriesAsync(context, auditEntries, cancellationToken);
            }
        }
        finally
        {
            // Guaranteed cleanup to prevent DbContext dictionary memory leaks
            _pendingAudits.TryRemove(context, out _);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            _pendingAudits.TryRemove(eventData.Context, out _);
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private async Task<List<PendingAuditEntry>> BuildAuditEntriesAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        var auditEntries = new List<PendingAuditEntry>();

        // Directly iterate over ChangeTracker without allocating a intermediate List via .ToList()
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            if (entry.Entity is AuditChangeLog or AuditLogDetail)
            {
                continue;
            }

            var entityType = entry.Metadata.ClrType;

            var (isAuditable, auditEntityName) =
                EntityTypeCache.GetOrAdd(entityType, ResolveAuditMetadata);

            if (!isAuditable)
            {
                continue;
            }

            var hierarchy = await hierarchyResolver.ResolveAsync(
                entry,
                context,
                cancellationToken);

            var isSelfRoot = hierarchy?.Root == null;

            var rootEntityType = hierarchy?.Root?.EntityType ?? auditEntityName;

            var rootEntityId = hierarchy?.Root?.EntityId ?? ResolvePrimaryKeyValue(entry);

            var changeLog = new AuditChangeLog
            {
                Id = Guid.NewGuid(),
                EntityType = auditEntityName,

                /*
                 * May temporarily contain an EF negative identity.
                 * It is corrected in SavedChangesAsync().
                 */
                EntityId = ResolvePrimaryKeyValue(entry),

                RootEntityType = rootEntityType,

                /*
                 * For a child entity this is already the real parent/root ID.
                 * For a newly-created root entity this may be temporary and is corrected after SaveChanges.
                 */
                RootEntityId = rootEntityId,

                Action = entry.State.ToString(),
                UserId = ResolveUserId(),
                TimestampUtc = DateTime.UtcNow,
                HierarchyJson = hierarchy == null
                    ? null
                    : JsonSerializer.Serialize(hierarchy)
            };

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey() ||
                    AutoIgnoredProperties.Contains(property.Metadata.Name))
                {
                    continue;
                }

                if (entry.State == EntityState.Modified && !property.IsModified)
                {
                    continue;
                }

                var propInfo = property.Metadata.PropertyInfo;

                if (propInfo != null)
                {
                    var (isIgnored, logOnAdded) = GetPropertyAuditMetadata(propInfo, entityType);

                    // 1. Check [IgnoreAudit]
                    if (isIgnored)
                    {
                        continue;
                    }

                    // 2. Check [LogOnAdded] when EntityState == Added
                    if (entry.State == EntityState.Added && !logOnAdded)
                    {
                        continue;
                    }
                }

                var rawOldValue = entry.State == EntityState.Added ? null : property.OriginalValue;
                var rawNewValue = entry.State == EntityState.Deleted ? null : property.CurrentValue;

                // 3. Skip logging if values are equivalent (e.g. null vs "" vs " ")
                if (entry.State == EntityState.Modified && AreValuesEquivalent(rawOldValue, rawNewValue))
                {
                    continue;
                }

                changeLog.Details.Add(
                    new AuditLogDetail
                    {
                        Id = Guid.NewGuid(),
                        AuditChangeLogId = changeLog.Id,
                        PropertyName = property.Metadata.Name,
                        OldValue = FormatPropertyValue(rawOldValue),
                        NewValue = FormatPropertyValue(rawNewValue)
                    });
            }

            if (entry.State == EntityState.Added ||
                changeLog.Details.Count > 0 ||
                entry.State == EntityState.Deleted)
            {
                auditEntries.Add(
                    new PendingAuditEntry
                    {
                        Entry = entry,
                        AuditLog = changeLog,
                        IsSelfRoot = isSelfRoot
                    });
            }
        }

        return auditEntries;
    }

    private static (bool IsIgnored, bool LogOnAdded) GetPropertyAuditMetadata(PropertyInfo propInfo, Type entityType)
    {
        return PropertyMetadataCache.GetOrAdd(propInfo, p =>
        {
            var isIgnored = IsPropertyAttributePresent<IgnoreAuditAttribute>(p, entityType);
            var logOnAdded = IsPropertyAttributePresent<LogOnAddedAttribute>(p, entityType);
            return (isIgnored, logOnAdded);
        });
    }

    private static bool IsPropertyAttributePresent<TAttr>(PropertyInfo propInfo, Type entityType)
        where TAttr : Attribute
    {
        // Direct attribute check on the domain property
        if (propInfo.GetCustomAttribute<TAttr>(inherit: true) != null)
            return true;

        // MetadataType check for scaffolded DB-First entities
        var metadataTypeAttr = entityType.GetCustomAttribute<MetadataTypeAttribute>(inherit: true);
        if (metadataTypeAttr != null)
        {
            var metaProp = metadataTypeAttr.MetadataClassType.GetProperty(propInfo.Name);
            return metaProp?.GetCustomAttribute<TAttr>(inherit: true) != null;
        }

        return false;
    }

    private async Task SaveAuditEntriesAsync(
        DbContext mainContext,
        List<PendingAuditEntry> auditEntries,
        CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var auditContext = scope.ServiceProvider
            .GetRequiredService<ChangeAuditDbContext>();

        var connection = mainContext.Database.GetDbConnection();

        auditContext.Database.SetDbConnection(connection);

        var transaction = mainContext.Database.CurrentTransaction;

        if (transaction != null)
        {
            await auditContext.Database.UseTransactionAsync(
                transaction.GetDbTransaction(),
                cancellationToken);
        }

        auditContext.AuditChangeLogs.AddRange(auditEntries.Select(x => x.AuditLog));

        await auditContext.SaveChangesAsync(cancellationToken);
    }

    private static (bool IsAuditable, string EntityName) ResolveAuditMetadata(Type entityType)
    {
        var directAttr = entityType.GetCustomAttribute<AuditableAttribute>(inherit: true);

        if (directAttr != null)
        {
            return (true, directAttr.EntityTypeName ?? entityType.Name);
        }

        var metadataTypeAttr = entityType.GetCustomAttribute<MetadataTypeAttribute>(inherit: true);

        if (metadataTypeAttr != null)
        {
            var metaAttr = metadataTypeAttr.MetadataClassType
                .GetCustomAttribute<AuditableAttribute>(inherit: true);

            if (metaAttr != null)
            {
                return (true, metaAttr.EntityTypeName ?? entityType.Name);
            }
        }

        return (false, entityType.Name);
    }

    private static string ResolvePrimaryKeyValue(EntityEntry entry)
    {
        var primaryKey = entry.Metadata.FindPrimaryKey();

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
        if (Guid.TryParse(userContext.UserId, out var userId))
        {
            return userId;
        }

        throw new InvalidOperationException(
            "A valid user ID is required for audit logging.");
    }

    private static string? FormatPropertyValue(object? value)
    {
        if (value is null)
            return null;

        if (value is string str)
            return str;

        return JsonSerializer.Serialize(value);
    }

    private static string? NormalizeValue(object? value)
    {
        if (value is null)
            return null;

        if (value is string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str.Trim();
        }

        var formatted = FormatPropertyValue(value);
        return string.IsNullOrWhiteSpace(formatted) ? null : formatted.Trim();
    }

    private static bool AreValuesEquivalent(object? oldValue, object? newValue)
    {
        var normalizedOld = NormalizeValue(oldValue);
        var normalizedNew = NormalizeValue(newValue);

        return string.Equals(normalizedOld, normalizedNew, StringComparison.Ordinal);
    }

    private sealed class PendingAuditEntry
    {
        public required EntityEntry Entry { get; init; }

        public required AuditChangeLog AuditLog { get; init; }

        public bool IsSelfRoot { get; init; }
    }
}