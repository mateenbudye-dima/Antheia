namespace Dima.ChangeAudit.Models;

public class AuditChangeLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "Insert", "Update", "Delete"
    public Guid UserId { get; set; }
    //public string? UserRoles { get; set; }
    //public string? Path { get; set; }
    //public string? HttpMethod { get; set; }
    //public string? IpAddress { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public List<AuditLogDetail> Details { get; set; } = [];
}

