namespace Dima.WorkFlowAuditMiddleware.Entities;

public class ApprovalWorkflow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public ApprovalWorkflowStatus Status { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewerComments { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAtUtc { get; set; }
}

public enum ApprovalWorkflowStatus
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 5
}