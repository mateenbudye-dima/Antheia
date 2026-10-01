using Dima.ChangeAudit.Models.Resolvers;
using Microsoft.EntityFrameworkCore;

namespace Dima.ChangeAudit.Abstractions
{
    public interface IAuditContainerResolver
    {
        Task<AuditEntityReference?> ResolveAsync(
            int containerId,
            byte containerTypeId,
            DbContext context,
            CancellationToken cancellationToken = default);
    }
}
