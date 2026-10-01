namespace Dima.ChangeAudit.Models.Domain;

public class AuditLogDetail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AuditChangeLogId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public AuditChangeLog? AuditChangeLog { get; set; }
}


