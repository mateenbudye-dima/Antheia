using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Antheia.Domain.Entities;
using Antheia.Domain.Enums;
using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Models.Resolvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Antheia.Infrastructure.Resolvers;

public sealed class ApplicationAuditHierarchyResolver : IAuditHierarchyResolver
{
    private readonly IAuditContainerResolver _containerResolver;

    public ApplicationAuditHierarchyResolver(
        IAuditContainerResolver containerResolver)
    {
        _containerResolver = containerResolver;
    }

    public Task<AuditHierarchy?> ResolveAsync(
        EntityEntry entry,
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        var entityType = entry.Metadata.ClrType;

        if (entityType == typeof(Ingredient))
        {
            return ResolveSectionChildAsync(
                entry,
                nameof(Ingredient.SectionId),
                context,
                cancellationToken);
        }

        if (entityType == typeof(Evaluation))
        {
            return ResolveSectionChildAsync(
                entry,
                nameof(Evaluation.SectionId),
                context,
                cancellationToken);
        }

        if (entityType == typeof(PreparationMethod))
        {
            return ResolveSectionChildAsync(
                entry,
                nameof(PreparationMethod.SectionId),
                context,
                cancellationToken);
        }

        if (entityType == typeof(SectionRecord))
        {
            return ResolveSectionAsync(
                entry,
                context,
                cancellationToken);
        }

        return Task.FromResult<AuditHierarchy?>(null);
    }

    private async Task<AuditHierarchy?> ResolveSectionChildAsync(
        EntityEntry entry,
        string sectionPropertyName,
        DbContext context,
        CancellationToken cancellationToken)
    {
        var sectionId = GetIntValue(
            entry,
            sectionPropertyName);

        if (sectionId == null)
            return null;

        var cache = BatchHierarchyCache.GetOrCreate(context);

        // 1. Check Batch Cache
        if (!cache.Sections.TryGetValue(sectionId.Value, out var section))
        {
            // 2. Check EF Core Local Memory (0 DB Queries)
            section = context.Set<SectionRecord>().Local
                .FirstOrDefault(x => x.SectionId == sectionId.Value);

            // 3. Fallback to DB Query if untracked
            if (section == null)
            {
                section = await context.Set<SectionRecord>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.SectionId == sectionId.Value,
                        cancellationToken);
            }

            if (section != null)
            {
                cache.Sections[sectionId.Value] = section;
            }
        }

        if (section == null)
            return null;

        var container = await _containerResolver.ResolveAsync(
            section.ContainerId,
            (byte)section.ContainerTypeId,
            context,
            cancellationToken);

        // Resolve Section Title or fallback to SectionType enum string
        var sectionName = !string.IsNullOrWhiteSpace(section.SectionTitle)
            ? section.SectionTitle
            : section.SectionTypeId.ToString();

        // Optional: Extract Name/Title from child entity if available (e.g. Ingredient Name)
        var childName = GetEntityDisplayName(entry);

        return new AuditHierarchy
        {
            Root = container,
            // Explicitly describe the child entity being audited
            Target = new AuditEntityReference
            {
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = ResolvePrimaryKeyValue(entry),
                Name = childName
            },
            // Section acts as the intermediate parent
            Parents =
            [
                new AuditEntityReference
                {
                    EntityType = "Section",
                    EntityId = section.SectionId.ToString(),
                    Name = sectionName,
                    Metadata = new Dictionary<string, string>
                    {
                        { "SectionType", section.SectionTypeId.ToString() },
                        { "SectionTypeId", ((int)section.SectionTypeId).ToString() }
                    }
                }
            ]
        };
    }

    private static string? GetEntityDisplayName(EntityEntry entry)
    {
        // Suffixes to check against property names
        string[] candidateSuffixes = ["Name", "Title", "Description"];

        foreach (var suffix in candidateSuffixes)
        {
            // Find the first property whose name ends with the suffix
            var property = entry.Metadata.GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

            if (property != null)
            {
                var value = entry.Property(property.Name).CurrentValue
                         ?? entry.Property(property.Name).OriginalValue;

                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return value.ToString();
                }
            }
        }

        return null;
    }

    private async Task<AuditHierarchy?> ResolveSectionAsync(
        EntityEntry entry,
        DbContext context,
        CancellationToken cancellationToken)
    {
        var containerId = GetIntValue(
            entry,
            nameof(SectionRecord.ContainerId));

        var containerTypeId = GetIntValue(
            entry,
            nameof(SectionRecord.ContainerTypeId));

        if (containerId == null || containerTypeId == null)
            return null;

        var container = await _containerResolver.ResolveAsync(
            containerId.Value,
            (byte)containerTypeId.Value,
            context,
            cancellationToken);

        var sectionName = entry.Property("SectionTitle")?.CurrentValue?.ToString();
        string? sectionTypeString = null;
        string? sectionTypeIdString = null;

        var sectionTypeProperty = entry.Property("SectionTypeId")?.CurrentValue
            ?? entry.Property("SectionType")?.OriginalValue;

        if (sectionTypeProperty != null)
        {
            if (sectionTypeProperty is SectionType sectionType)
            {
                sectionTypeString = sectionType.ToString();
                sectionTypeIdString = ((int)sectionType).ToString();
            }
            else if (int.TryParse(sectionTypeProperty.ToString(), out int sectionTypeValue))
            {
                var parsedType = (SectionType)sectionTypeValue;
                sectionTypeString = parsedType.ToString();
                sectionTypeIdString = sectionTypeValue.ToString();
            }
        }

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = sectionTypeString;
        }

        var metadata = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(sectionTypeString))
            metadata["SectionType"] = sectionTypeString;
        if (!string.IsNullOrEmpty(sectionTypeIdString))
            metadata["SectionTypeId"] = sectionTypeIdString;

        return new AuditHierarchy
        {
            Root = container,
            // Section itself is the Target
            Target = new AuditEntityReference
            {
                EntityType = "Section",
                EntityId = ResolvePrimaryKeyValue(entry),
                Name = sectionName,
                Metadata = metadata
            },
            Parents = [] // Pure container relationship: Blend > Section
        };
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
                    .ToString()
                ?? entry.Property(property.Name)
                    .OriginalValue?
                    .ToString()
                ?? "0");

        return string.Join(",", values);
    }

    private static int? GetIntValue(
        EntityEntry entry,
        string propertyName)
    {
        var property = entry.Property(propertyName);

        var value = entry.State == EntityState.Deleted
            ? property.OriginalValue
            : property.CurrentValue ?? property.OriginalValue;

        return value == null
            ? null
            : Convert.ToInt32(value);
    }

    // Helper class scoped to the DbContext execution life
    internal sealed class BatchHierarchyCache
    {
        private static readonly ConditionalWeakTable<DbContext, BatchHierarchyCache> _caches = new();

        public Dictionary<int, SectionRecord> Sections { get; } = new();
        public Dictionary<(int ContainerId, byte ContainerTypeId), AuditEntityReference> Containers { get; } = new();

        public static BatchHierarchyCache GetOrCreate(DbContext context)
        {
            return _caches.GetOrCreateValue(context);
        }
    }
}