namespace Dima.ChangeAudit.Models.Application;

public sealed class AuditLogQuery
{
    public string RootEntityType { get; init; } = string.Empty;

    public string RootEntityId { get; init; } = string.Empty;

    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }

    public string? EntityType { get; init; }

    public string? Action { get; init; }

    public Guid? UserId { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 50;
}