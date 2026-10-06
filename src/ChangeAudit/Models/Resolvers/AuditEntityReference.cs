using System;
using System.Collections.Generic;
using System.Text;

namespace Dima.ChangeAudit.Models.Resolvers
{
    public sealed class AuditEntityReference
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
