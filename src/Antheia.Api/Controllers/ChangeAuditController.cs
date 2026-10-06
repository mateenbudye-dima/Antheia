using Antheia.Domain.Entities;
using Antheia.Domain.Enums;
using Asp.Versioning;
using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Models.Application;
using Dima.ChangeAudit.Models.Display;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Antheia.Api.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ChangeAuditController(
    IAuditLogService auditLogService,
    IAuditLogDisplayFormatter auditLogDisplayFormatter) : ControllerBase
{

    [HttpGet("logs/{rootEntityType}/{rootEntityId}")]
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


    [HttpGet("blend/{rootEntityId}")]
    public async Task<ActionResult<FormattedAuditLogPagedResult>> GetForBlend(
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
            RootEntityType = ChangeAuditRootEntityType.Blend.ToString(),
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

        var filteredItems = result.Items
                            .Where(i => !(i.EntityType == nameof(PreparationMethod) && i.Action == "Added"))
                            .ToList();

        var filteredResult = new AuditLogPagedResult
                            {
                                Items = filteredItems,
                                PageNumber = result.PageNumber,
                                PageSize = result.PageSize,
                                TotalCount = result.TotalCount - (result.Items.Count - filteredItems.Count) // Adjust total count
                            };

        FormattedAuditLogPagedResult displayResult = auditLogDisplayFormatter.FormatPaged(filteredResult);

        return Ok(displayResult);
    }
}