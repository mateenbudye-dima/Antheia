
using Dima.ChangeAudit.Models.Application;
namespace Dima.ChangeAudit.Abstractions;

public interface IAuditLogService
{
    Task<AuditLogPagedResult> GetLogsAsync(
        AuditLogQuery query,
        CancellationToken cancellationToken = default);
}