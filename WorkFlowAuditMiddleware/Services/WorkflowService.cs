using Dima.WorkFlowAuditMiddleware.Data;
using Dima.WorkFlowAuditMiddleware.Entities;
using Dima.WorkFlowAuditMiddleware.Models;
using Microsoft.EntityFrameworkCore;

namespace Dima.WorkFlowAuditMiddleware.Services;

public class WorkflowService : IWorkflowService
{
    private readonly AuditDbContext _auditDbContext;

    public WorkflowService(AuditDbContext auditDbContext)
    {
        _auditDbContext = auditDbContext;
    }

    public async Task SubmitForApprovalAsync(string entityType, long entityId, Guid userId, SubmittedFor submittedFor)
    {
        var workflow = new ApprovalWorkflow
        {
            EntityType = entityType,
            EntityId = entityId,
            Status = submittedFor == SubmittedFor.Approve ? ApprovalWorkflowStatus.ForApproval : ApprovalWorkflowStatus.ForReview,
            RequestedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _auditDbContext.ApprovalWorkflows.Add(workflow);
        await _auditDbContext.SaveChangesAsync();
    }

    public async Task ApproveAsync(string entityType, long entityId, Guid reviewerId, SubmittedFor submittedFor, string? comments)
    {
        var workflow = await _auditDbContext.ApprovalWorkflows
            .Where(w => w.EntityType == entityType && w.EntityId == entityId && (w.Status == ApprovalWorkflowStatus.ForReview || w.Status == ApprovalWorkflowStatus.ForApproval))
            .OrderByDescending(w => w.CreatedAtUtc)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException($"No active workflow found for {entityType} {entityId}");

        workflow.Status = submittedFor == SubmittedFor.Approve? ApprovalWorkflowStatus.Approved: ApprovalWorkflowStatus.Reviewed;
        workflow.ReviewedByUserId = reviewerId;
        workflow.ReviewerComments = comments;
        workflow.ReviewedAtUtc = DateTime.UtcNow;

        await _auditDbContext.SaveChangesAsync();
    }

    public async Task RejectAsync(string entityType, long entityId, Guid reviewerId, string? comments)
    {
        var workflow = await _auditDbContext.ApprovalWorkflows
            .Where(w => w.EntityType == entityType && w.EntityId == entityId && (w.Status == ApprovalWorkflowStatus.ForReview || w.Status == ApprovalWorkflowStatus.ForApproval))
            .OrderByDescending(w => w.CreatedAtUtc)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException($"No active workflow found for {entityType} {entityId}");

        workflow.Status = ApprovalWorkflowStatus.Rejected;
        workflow.ReviewedByUserId = reviewerId;
        workflow.ReviewerComments = comments;
        workflow.ReviewedAtUtc = DateTime.UtcNow;

        await _auditDbContext.SaveChangesAsync();
    }

    public async Task CancelSubmissionAsync(string entityType, long entityId, Guid reviewerId, string? comments)
    {
        var workflow = await _auditDbContext.ApprovalWorkflows
            .Where(w => w.EntityType == entityType && w.EntityId == entityId && (w.Status == ApprovalWorkflowStatus.ForReview || w.Status == ApprovalWorkflowStatus.ForApproval))
            .OrderByDescending(w => w.CreatedAtUtc)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException($"No active workflow found for {entityType} {entityId}");

        workflow.Status = ApprovalWorkflowStatus.Cancelled;
        workflow.ReviewedByUserId = reviewerId;
        workflow.ReviewerComments = comments;
        workflow.ReviewedAtUtc = DateTime.UtcNow;

        await _auditDbContext.SaveChangesAsync();
    }


    /// <summary>
    /// Retrieves paginated and filtered Audit Logs sorted by newest first.
    /// Uses AsNoTracking for read-only query speed.
    /// </summary>
    public async Task<PagedResult<AuditLog>> GetAuditLogsAsync(AuditLogFilterParameters filter)
    {
        var query = _auditDbContext.AuditLogs.AsNoTracking().AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.ProjectId))
            query = query.Where(x => x.ProjectId == filter.ProjectId);

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(x => x.EntityType == filter.EntityType);

        if (filter.EntityId.HasValue)
            query = query.Where(x => x.EntityId == filter.EntityId.Value);

        if (filter.UserId.HasValue)
            query = query.Where(x => x.UserId == filter.UserId.Value);

        if (filter.FromUtc.HasValue)
            query = query.Where(x => x.TimestampUtc >= filter.FromUtc.Value);

        if (filter.ToUtc.HasValue)
            query = query.Where(x => x.TimestampUtc <= filter.ToUtc.Value);

        // Get total count for pagination metadata
        var totalCount = await query.CountAsync();

        // Execute paginated query usingIX_AuditLogs_TimestampUtc index
        var items = await query
            .OrderByDescending(x => x.TimestampUtc)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    /// <summary>
    /// Get single Audit Log by Id.
    /// </summary>
    public async Task<AuditLog?> GetAuditLogByIdAsync(Guid id)
    {
        return await _auditDbContext.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Get active approval workflow state for a specific entity.
    /// Hits the IX_ApprovalWorkflows_EntityType_EntityId unique index.
    /// </summary>
    public async Task<ApprovalWorkflow?> GetWorkflowByEntityAsync(string entityType, long entityId)
    {
        return await _auditDbContext.ApprovalWorkflows
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EntityType == entityType && x.EntityId == entityId);
    }

    /// <summary>
    /// Get all workflows currently pending review (Status = 1 / Pending).
    /// </summary>
    public async Task<IEnumerable<ApprovalWorkflow>> GetPendingWorkflowsAsync()
    {
        return await _auditDbContext.ApprovalWorkflows
            .AsNoTracking()
            .Where(x => x.Status == ApprovalWorkflowStatus.ForReview || x.Status == ApprovalWorkflowStatus.ForApproval)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }
}