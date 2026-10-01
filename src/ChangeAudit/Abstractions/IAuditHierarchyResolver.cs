using Dima.ChangeAudit.Models.Resolvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dima.ChangeAudit.Abstractions
{
    public interface IAuditHierarchyResolver
    {
        Task<AuditHierarchy?> ResolveAsync(
            EntityEntry entry,
            DbContext context,
            CancellationToken cancellationToken = default);
    }
}
