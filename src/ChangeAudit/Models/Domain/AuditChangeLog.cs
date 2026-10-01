namespace Dima.ChangeAudit.Models.Domain;

public class AuditChangeLog
{
    public Guid Id { get; set; }

    public string EntityType { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string RootEntityType { get; set; } = string.Empty;

    public string RootEntityId { get; set; } = string.Empty;

    public string Action { get; set; } = null!;

    public Guid UserId { get; set; }

    public DateTime TimestampUtc { get; set; }

    public string? HierarchyJson { get; set; }

    public ICollection<AuditLogDetail> Details { get; set; }
        = new List<AuditLogDetail>();
}

