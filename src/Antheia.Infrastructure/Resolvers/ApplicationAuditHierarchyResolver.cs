using Antheia.Domain.Entities;
using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Models.Resolvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Antheia.Infrastructure.Resolvers;

public sealed class ApplicationAuditHierarchyResolver
    : IAuditHierarchyResolver
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

        var section = await context.Set<SectionRecord>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.SectionId == sectionId.Value,
                cancellationToken);

        if (section == null)
            return null;

        var container = await _containerResolver.ResolveAsync(
            section.ContainerId,
            (byte)section.ContainerTypeId,
            context,
            cancellationToken);

        return new AuditHierarchy
        {
            Root = container,
            Parents =
            [
                new AuditEntityReference
                {
                    EntityType = "Section",
                    EntityId = section.SectionId.ToString()
                }
            ]
        };
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

        return new AuditHierarchy
        {
            Root = container
        };
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
}