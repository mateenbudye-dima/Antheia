namespace Dima.WorkflowAuditing.Entities;

public class ApprovalWorkflow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // "Draft", "PendingApproval", "Approved", "Rejected"
    public string RequestedByUserId { get; set; } = string.Empty;
    public string? ReviewedByUserId { get; set; }
    public string? ReviewerComments { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAtUtc { get; set; }
}