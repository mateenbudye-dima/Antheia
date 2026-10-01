namespace Dima.ChangeAudit.Models.Application;

public sealed class AuditLogDetailResult
{
    public string PropertyName { get; init; } = string.Empty;

    public string? OldValue { get; init; }

    public string? NewValue { get; init; }
}