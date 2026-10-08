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
    ForReview = 1,
    Reviewed = 2,
    ForApproval = 3,
    Approved = 4,
    Rejected = 5,
    Cancelled = 6,
}

public enum SubmittedFor
{
    Review= 1,
    Approve =2,
}