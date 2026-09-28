using Dima.WorkFlowAuditMiddleware.Entities;
using Dima.WorkFlowAuditMiddleware.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dima.WorkFlowAuditMiddleware.Services
{
    public interface IWorkflowService
    {
        Task SubmitForApprovalAsync(string entityType, long entityId, Guid userId);
        Task ApproveAsync(string entityType, long entityId, Guid reviewerId, string? comments);
        Task RejectAsync(string entityType, long entityId, Guid reviewerId, string? comments);

        // --- Audit Logs ---
        Task<PagedResult<AuditLog>> GetAuditLogsAsync(AuditLogFilterParameters filter);
        Task<AuditLog?> GetAuditLogByIdAsync(Guid id);

        // --- Approval Workflows ---
        Task<ApprovalWorkflow?> GetWorkflowByEntityAsync(string entityType, long entityId);
        Task<IEnumerable<ApprovalWorkflow>> GetPendingWorkflowsAsync();
    }
}
