using System;
using System.Collections.Generic;
using System.Text;

namespace Dima.WorkFlowAuditMiddleware.Services
{
    public interface IWorkflowService
    {
        Task SubmitForApprovalAsync(string entityType, string entityId, string userId);
        Task ApproveAsync(string entityType, string entityId, string reviewerId, string? comments);
        Task RejectAsync(string entityType, string entityId, string reviewerId, string? comments);
    }
}
