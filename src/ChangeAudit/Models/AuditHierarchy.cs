using System;
using System.Collections.Generic;
using System.Text;

namespace ChangeAudit.Models
{
    public sealed class AuditHierarchy
    {
        public AuditEntityReference? Root { get; init; }

        public IReadOnlyList<AuditEntityReference> Parents { get; init; }
            = [];
    }
}
