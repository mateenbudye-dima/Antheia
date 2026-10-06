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

    private static readonly ConcurrentDictionary<
        PropertyInfo,
        bool> PropertyIgnoreCache = new();

    private static readonly ConcurrentDictionary<
        PropertyInfo,
        bool> LogOnAddedCache = new();

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
        if (eventData.Context is null)
            return await base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);

        var context = eventData.Context;

        if (context is ChangeAuditDbContext)
            return await base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);

        var auditEntries = await BuildAuditEntriesAsync(
            context,
            cancellationToken);

        if (auditEntries.Count > 0)
        {
            _pendingAudits[context] = auditEntries;
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

        var context = eventData.Context;

        if (context is ChangeAuditDbContext)
        {
            return await base.SavedChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        if (!_pendingAudits.TryRemove(
                context,
                out var auditEntries))
        {
            return await base.SavedChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        foreach (var pendingAudit in auditEntries)
        {
            var audit = pendingAudit.AuditLog;

            audit.EntityId = ResolvePrimaryKeyValue(
                pendingAudit.Entry);

            /*
             * If the changed entity itself is the root,
             * its RootEntityId must also use the real generated ID.
             */
            if (pendingAudit.IsSelfRoot)
            {
                audit.RootEntityId = audit.EntityId;
            }
        }

        await SaveAuditEntriesAsync(
            context,
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
            _pendingAudits.TryRemove(
                eventData.Context,
                out _);
        }

        return base.SaveChangesFailedAsync(
            eventData,
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
            var entityType = entry.Metadata.ClrType;

            var (isAuditable, auditEntityName) =
                EntityTypeCache.GetOrAdd(
                    entityType,
                    ResolveAuditMetadata);

            if (!isAuditable)
                continue;

            var hierarchy =
                await hierarchyResolver.ResolveAsync(
                    entry,
                    context,
                    cancellationToken);

            var isSelfRoot = hierarchy?.Root == null;

            var rootEntityType =
                hierarchy?.Root?.EntityType
                ?? auditEntityName;

            var rootEntityId =
                hierarchy?.Root?.EntityId
                ?? ResolvePrimaryKeyValue(entry);

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
                 * For a child entity this is already the real
                 * parent/root ID.
                 *
                 * For a newly-created root entity this may be
                 * temporary and is corrected after SaveChanges.
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

                // 1. Check [IgnoreAudit] attribute
                if (propInfo != null &&
                    PropertyIgnoreCache.GetOrAdd(
                        propInfo,
                        p => IsPropertyAttributePresent<IgnoreAuditAttribute>(p, entityType)))
                {
                    continue;
                }

                // 2. Check [LogOnAdded] attribute when EntityState == Added
                if (entry.State == EntityState.Added)
                {
                    var shouldLogOnAdded = propInfo != null &&
                        LogOnAddedCache.GetOrAdd(
                            propInfo,
                            p => IsPropertyAttributePresent<LogOnAddedAttribute>(p, entityType));

                    // Skip logging property on insert unless explicitly decorated with [LogOnAdded]
                    if (!shouldLogOnAdded)
                    {
                        continue;
                    }
                }

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

                        PropertyName = property.Metadata.Name,

                        OldValue = FormatPropertyValue(oldValue),

                        NewValue = FormatPropertyValue(newValue)
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

        var auditContext =
            scope.ServiceProvider
                .GetRequiredService<ChangeAuditDbContext>();

        var connection =
            mainContext.Database.GetDbConnection();

        auditContext.Database.SetDbConnection(connection);

        var transaction =
            mainContext.Database.CurrentTransaction;

        if (transaction != null)
        {
            await auditContext.Database.UseTransactionAsync(
                transaction.GetDbTransaction(),
                cancellationToken);
        }

        auditContext.AuditChangeLogs.AddRange(
            auditEntries.Select(x => x.AuditLog));

        await auditContext.SaveChangesAsync(
            cancellationToken);
    }

    private static (bool IsAuditable, string EntityName)
        ResolveAuditMetadata(Type entityType)
    {
        var directAttr =
            entityType.GetCustomAttribute<AuditableAttribute>(
                inherit: true);

        if (directAttr != null)
        {
            return (
                true,
                directAttr.EntityTypeName ?? entityType.Name);
        }

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

        return (false, entityType.Name);
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

    private static string? FormatPropertyValue(object? value)
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

        public bool IsSelfRoot { get; init; }
    }
}