namespace Dima.WorkflowAuditing.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuditWorkflowAttribute : Attribute
{
    public string EntityType { get; }
    public string RequiredApprovalRole { get; }

    public AuditWorkflowAttribute(string entityType, string requiredApprovalRole = "Approver")
    {
        EntityType = entityType;
        RequiredApprovalRole = requiredApprovalRole;
    }
}