using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Models.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Antheia.Api.Controllers;

[ApiController]
[Route("api/changeaudit")]
[Authorize]
public class ChangeAuditController(
    IAuditLogService auditLogService) : ControllerBase
{
    [HttpGet("{rootEntityType}/{rootEntityId}")]
    public async Task<ActionResult<AuditLogPagedResult>> GetLogs(
        string rootEntityType,
        string rootEntityId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? entityType = null,
        [FromQuery] string? action = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = new AuditLogQuery
        {
            RootEntityType = rootEntityType,
            RootEntityId = rootEntityId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            EntityType = entityType,
            Action = action,
            UserId = userId,
            FromUtc = fromUtc,
            ToUtc = toUtc
        };

        var result = await auditLogService.GetLogsAsync(
            query,
            cancellationToken);

        return Ok(result);
    }
}