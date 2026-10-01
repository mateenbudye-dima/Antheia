using Antheia.Domain.Entities;
using ChangeAudit.Abstractions;
using ChangeAudit.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Infrastructure.Resolvers
{
    public sealed class ApplicationAuditHierarchyResolver
    : IAuditHierarchyResolver
    {
        private readonly IAuditContainerResolver _containerResolver;
        public ApplicationAuditHierarchyResolver(
        IAuditContainerResolver containerResolver)
        {
            _containerResolver = containerResolver;
        }
        public async Task<AuditHierarchy?> ResolveAsync(
            EntityEntry entry,
            DbContext context,
            CancellationToken cancellationToken = default)
        {
            return entry.Metadata.ClrType.Name switch
            {
                nameof(Ingredient)
                    => await ResolveSectionIngredientAsync(
                        entry,
                        context,
                        cancellationToken),

                nameof(SectionRecord)
                    => await ResolveSectionAsync(
                        entry,
                        context,
                        cancellationToken),

                _ => null
            };
        }

        private async Task<AuditHierarchy?> ResolveSectionIngredientAsync(
            EntityEntry entry,
            DbContext context,
            CancellationToken cancellationToken)
        {
            var sectionId = GetIntValue(
                entry,
                nameof(Ingredient.SectionId));

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
            var sectionId = GetIntValue(
                entry,
                nameof(SectionRecord.SectionId));

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
                Root = container
            };
        }

        private static int? GetIntValue(
            EntityEntry entry,
            string propertyName)
        {
            var property = entry.Property(propertyName);

            var value = property.CurrentValue ??
                        property.OriginalValue;

            if (value == null)
                return null;

            return Convert.ToInt32(value);
        }
    }
}
