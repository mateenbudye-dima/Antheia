using Antheia.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Antheia.Infrastructure.Services;

public class UserCacheInvalidationInterceptor : SaveChangesInterceptor
{
    private readonly IUserProfileCacheService _profileCache;

    public UserCacheInvalidationInterceptor(IUserProfileCacheService profileCache)
    {
        _profileCache = profileCache;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Invalidate(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Invalidate(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Invalidate(DbContext? context)
    {
        if (context == null) return;

        try
        {
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            var userIds = new HashSet<Guid>();

            foreach (var entry in entries)
            {
                var typeName = entry.Entity?.GetType().Name;

                if (typeName == nameof(Antheia.Infrastructure.Entities.AspnetUser))
                {
                    var id = entry.CurrentValues.TryGetValue<Guid?>("UserId", out var maybeId) ? maybeId : null;
                    if (id.HasValue) userIds.Add(id.Value);
                }

                // DepartmentUser changes affect user->department mapping
                if (typeName == nameof(Antheia.Domain.CondorEntities.DepartmentUser))
                {
                    var userId = entry.CurrentValues.TryGetValue<Guid?>("UserId", out var maybeUid) ? maybeUid : null;
                    if (userId.HasValue) userIds.Add(userId.Value);
                }
            }

            foreach (var userId in userIds)
            {
                // Fire-and-forget invalidation
                _ = _profileCache.RemoveUserProfileAsync(userId);
            }
        }
        catch
        {
            // Don't let cache invalidation block DB save
        }
    }
}
