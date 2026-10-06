namespace Dima.ChangeAudit.Models.Display;

public sealed class FormattedAuditLogPagedResult
{
    public IReadOnlyList<FormattedAuditLogResult> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public sealed class FormattedAuditLogResult
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string RootEntityType { get; init; } = string.Empty;
    public string RootEntityId { get; init; } = string.Empty;
    public string? ParentPath { get; init; }
    public Guid UserId { get; init; }
    public DateTime TimestampUtc { get; init; }
    public List<string> DetailChanges { get; init; } = [];
}

internal sealed class AuditHierarchyInfo
{
    public AuditEntityRef? Root { get; init; }
    public AuditEntityRef? Target { get; init; }
    public List<AuditEntityRef> Parents { get; init; } = [];
}

internal sealed class AuditEntityRef
{
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string? Name { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}