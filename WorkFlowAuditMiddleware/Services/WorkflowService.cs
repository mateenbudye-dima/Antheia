using Microsoft.EntityFrameworkCore;
using Dima.WorkflowAuditing.Data;
using Dima.WorkflowAuditing.Entities;

namespace Dima.WorkFlowAuditMiddleware.Services;

public class WorkflowService : IWorkflowService
{
    private readonly AuditDbContext _auditDbContext;

    public WorkflowService(AuditDbContext auditDbContext)
    {
        _auditDbContext = auditDbContext;
    }

    public async Task SubmitForApprovalAsync(string entityType, string entityId, string userId)
    {
        var workflow = new ApprovalWorkflow
        {
            EntityType = entityType,
            EntityId = entityId,
            Status = "PendingApproval",
            RequestedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _auditDbContext.ApprovalWorkflows.Add(workflow);
        await _auditDbContext.SaveChangesAsync();
    }

    public async Task ApproveAsync(string entityType, string entityId, string reviewerId, string? comments)
    {
        var workflow = await _auditDbContext.ApprovalWorkflows
            .Where(w => w.EntityType == entityType && w.EntityId == entityId && w.Status == "PendingApproval")
            .OrderByDescending(w => w.CreatedAtUtc)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException($"No active workflow found for {entityType} {entityId}");

        workflow.Status = "Approved";
        workflow.ReviewedByUserId = reviewerId;
        workflow.ReviewerComments = comments;
        workflow.ReviewedAtUtc = DateTime.UtcNow;

        await _auditDbContext.SaveChangesAsync();
    }

    public async Task RejectAsync(string entityType, string entityId, string reviewerId, string? comments)
    {
        var workflow = await _auditDbContext.ApprovalWorkflows
            .Where(w => w.EntityType == entityType && w.EntityId == entityId && w.Status == "PendingApproval")
            .OrderByDescending(w => w.CreatedAtUtc)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException($"No active workflow found for {entityType} {entityId}");

        workflow.Status = "Rejected";
        workflow.ReviewedByUserId = reviewerId;
        workflow.ReviewerComments = comments;
        workflow.ReviewedAtUtc = DateTime.UtcNow;

        await _auditDbContext.SaveChangesAsync();
    }
}