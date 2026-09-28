using System;
using System.Collections.Generic;
using System.Text;

namespace Dima.WorkFlowAuditMiddleware.Models
{
    public class AuditLogFilterParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? ProjectId { get; set; }
        public string? EntityType { get; set; }
        public long? EntityId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
    }
}
