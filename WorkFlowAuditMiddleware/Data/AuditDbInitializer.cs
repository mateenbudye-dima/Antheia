using Microsoft.EntityFrameworkCore;

namespace Dima.WorkFlowAuditMiddleware.Data;

public static class AuditDbInitializer
{
    public static async Task InitializeAsync(AuditDbContext dbContext, CancellationToken cancellationToken = default)
    {
        const string sqlScript = """
            -- 1. Ensure Schema Exists
            IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'WorkflowAudit')
            BEGIN
                EXEC('CREATE SCHEMA [WorkflowAudit] AUTHORIZATION [dbo]');
            END;

            -- 2. Ensure AuditLogs Table Exists
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[WorkflowAudit].[AuditLogs]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [WorkflowAudit].[AuditLogs] (
                    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_AuditLogs] PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
                    [ProjectId] NVARCHAR(100) NOT NULL,
                    [EntityType] NVARCHAR(100) NOT NULL,
                    [EntityId] BIGINT NOT NULL,
                    [Action] NVARCHAR(50) NOT NULL,
                    [UserId] UNIQUEIDENTIFIER NOT NULL,
                    [UserRoles] NVARCHAR(500) NULL,
                    [Path] NVARCHAR(500) NOT NULL,
                    [HttpMethod] NVARCHAR(10) NOT NULL,
                    [IpAddress] NVARCHAR(45) NULL,
                    [TimestampUtc] DATETIME2(7) NOT NULL CONSTRAINT [DF_AuditLogs_TimestampUtc] DEFAULT SYSUTCDATETIME()
                );

                CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityType_EntityId] 
                    ON [WorkflowAudit].[AuditLogs] ([EntityType], [EntityId]);

                CREATE NONCLUSTERED INDEX [IX_AuditLogs_TimestampUtc] 
                    ON [WorkflowAudit].[AuditLogs] ([TimestampUtc] DESC);
            END;

            -- 3. Ensure ApprovalWorkflows Table Exists
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[WorkflowAudit].[ApprovalWorkflows]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [WorkflowAudit].[ApprovalWorkflows] (
                    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_ApprovalWorkflows] PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
                    [EntityType] NVARCHAR(100) NOT NULL,
                    [EntityId] BIGINT NOT NULL,
                    [Status] INT NOT NULL CONSTRAINT [DF_ApprovalWorkflows_Status] DEFAULT 0,
                    [RequestedByUserId] UNIQUEIDENTIFIER NOT NULL,
                    [ReviewedByUserId] UNIQUEIDENTIFIER NULL,
                    [ReviewerComments] NVARCHAR(1000) NULL,
                    [CreatedAtUtc] DATETIME2(7) NOT NULL CONSTRAINT [DF_ApprovalWorkflows_CreatedAtUtc] DEFAULT SYSUTCDATETIME(),
                    [ReviewedAtUtc] DATETIME2(7) NULL
                );

                CREATE NONCLUSTERED INDEX [IX_ApprovalWorkflows_EntityType_EntityId] 
                    ON [WorkflowAudit].[ApprovalWorkflows] ([EntityType], [EntityId]);
            END;
            """;

        await dbContext.Database.ExecuteSqlRawAsync(sqlScript, cancellationToken);
    }
}