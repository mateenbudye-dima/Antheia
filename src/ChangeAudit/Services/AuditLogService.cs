using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Data;
using Dima.ChangeAudit.Models.Application;
using Microsoft.EntityFrameworkCore;

namespace Dima.ChangeAudit.Services;

public sealed class AuditLogService(
    ChangeAuditDbContext dbContext) : IAuditLogService
{
    public async Task<AuditLogPagedResult> GetLogsAsync(
        AuditLogQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.RootEntityType))
        {
            throw new ArgumentException(
                "Root entity type is required.",
                nameof(query));
        }

        if (string.IsNullOrWhiteSpace(query.RootEntityId))
        {
            throw new ArgumentException(
                "Root entity ID is required.",
                nameof(query));
        }

        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var logsQuery = dbContext.AuditChangeLogs
            .AsNoTracking()
            .Where(x =>
                x.RootEntityType == query.RootEntityType &&
                x.RootEntityId == query.RootEntityId);

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            logsQuery = logsQuery.Where(
                x => x.EntityType == query.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            logsQuery = logsQuery.Where(
                x => x.Action == query.Action);
        }

        if (query.UserId.HasValue)
        {
            logsQuery = logsQuery.Where(
                x => x.UserId == query.UserId.Value);
        }

        if (query.FromUtc.HasValue)
        {
            logsQuery = logsQuery.Where(
                x => x.TimestampUtc >= query.FromUtc.Value);
        }

        if (query.ToUtc.HasValue)
        {
            logsQuery = logsQuery.Where(
                x => x.TimestampUtc <= query.ToUtc.Value);
        }

        var totalCount = await logsQuery.CountAsync(
            cancellationToken);

        var items = await logsQuery
            .OrderByDescending(x => x.TimestampUtc)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogResult
            {
                Id = x.Id,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                RootEntityType = x.RootEntityType,
                RootEntityId = x.RootEntityId,
                Action = x.Action,
                UserId = x.UserId,
                TimestampUtc = x.TimestampUtc,
                HierarchyJson = x.HierarchyJson,

                Details = x.Details
                    .OrderBy(d => d.PropertyName)
                    .Select(d => new AuditLogDetailResult
                    {
                        PropertyName = d.PropertyName,
                        OldValue = d.OldValue,
                        NewValue = d.NewValue
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new AuditLogPagedResult
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}