using System;
using System.Collections.Generic;
using System.Text;

namespace ChangeAudit.Models
{
    public sealed class AuditEntityReference
    {
        public string EntityType { get; init; } = null!;
        public string EntityId { get; init; } = null!;
    }
}
