namespace Dima.ChangeAudit.Models.Application;

public sealed class AuditLogResult
{
    public Guid Id { get; init; }

    public string EntityType { get; init; } = string.Empty;

    public string EntityId { get; init; } = string.Empty;
    public string RootEntityType { get; init; } = string.Empty;

    public string RootEntityId { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public Guid UserId { get; init; }

    public DateTime TimestampUtc { get; init; }

    public string? HierarchyJson { get; init; }

    public List<AuditLogDetailResult> Details { get; init; } = [];
}