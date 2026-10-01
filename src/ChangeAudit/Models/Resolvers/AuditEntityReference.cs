using System;
using System.Collections.Generic;
using System.Text;

namespace Dima.ChangeAudit.Models.Resolvers
{
    public sealed class AuditEntityReference
    {
        public string EntityType { get; init; } = null!;
        public string EntityId { get; init; } = null!;
    }
}
