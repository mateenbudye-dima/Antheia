namespace Dima.ChangeAudit.Abstractions;

public interface IAuditableEntity
{
    string EntityType => GetType().Name;
    string EntityId { get; }
}