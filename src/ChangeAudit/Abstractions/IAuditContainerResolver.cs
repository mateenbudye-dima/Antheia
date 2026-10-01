using ChangeAudit.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChangeAudit.Abstractions
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
