using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antheia.Domain.Entities;
using Antheia.Domain.Enums;
using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Models.Resolvers;
using Microsoft.EntityFrameworkCore;
using static Antheia.Infrastructure.Resolvers.ApplicationAuditHierarchyResolver;

namespace Antheia.Infrastructure.Resolvers;

public sealed class AuditContainerResolver : IAuditContainerResolver
{
    public async Task<AuditEntityReference?> ResolveAsync(
        int containerId,
        byte containerTypeId,
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = (containerId, containerTypeId);
        var cache = BatchHierarchyCache.GetOrCreate(context);

        // 1. Check Batch Cache
        if (cache.Containers.TryGetValue(cacheKey, out var cachedReference))
        {
            return cachedReference;
        }

        AuditEntityReference? reference = containerTypeId switch
        {
            (byte)SectionContainerType.Blend =>
                await ResolveBlendAsync(
                    containerId,
                    context,
                    cancellationToken),

            _ => null
        };

        if (reference != null)
        {
            cache.Containers[cacheKey] = reference;
        }

        return reference;
    }

    private static async Task<AuditEntityReference?> ResolveBlendAsync(
        int blendId,
        DbContext context,
        CancellationToken cancellationToken)
    {
        // 1. Check EF Core Local Memory (0 DB Queries)
        var localBlend = context.Set<BlendRecord>().Local
            .FirstOrDefault(x => x.BlendId == blendId);

        if (localBlend != null)
        {
            return new AuditEntityReference
            {
                EntityType = "Blend",
                EntityId = blendId.ToString(),
                Name = localBlend.Code
            };
        }

        // 2. Direct Single Query (Replaces separate AnyAsync + Select query)
        var code = await context.Set<BlendRecord>()
            .AsNoTracking()
            .Where(x => x.BlendId == blendId)
            .Select(x => x.Code)
            .FirstOrDefaultAsync(cancellationToken);

        if (code == null)
            return null;

        return new AuditEntityReference
        {
            EntityType = "Blend",
            EntityId = blendId.ToString(),
            Name = code
        };
    }
}