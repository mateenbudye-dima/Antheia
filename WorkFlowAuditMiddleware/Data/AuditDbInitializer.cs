using Microsoft.EntityFrameworkCore;

namespace Dima.WorkflowAuditing.Data;

public static class AuditDbInitializer
{
    public static async Task InitializeAsync(AuditDbContext dbContext)
    {
        const string sqlScript = """
            IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'audit')
            BEGIN
                EXEC('CREATE SCHEMA [audit] AUTHORIZATION [dbo]');
            END;

            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit].[AuditLogs]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [audit].[AuditLogs] (
                    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_AuditLogs] PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
                    [ProjectId] NVARCHAR(100) NOT NULL,
                    [EntityType] NVARCHAR(100) NOT NULL,
                    [EntityId] NVARCHAR(100) NOT NULL,
                    [Action] NVARCHAR(50) NOT NULL,
                    [UserId] NVARCHAR(128) NOT NULL,
                    [UserRoles] NVARCHAR(500) NULL,
                    [Path] NVARCHAR(500) NOT NULL,
                    [HttpMethod] NVARCHAR(10) NOT NULL,
                    [IpAddress] NVARCHAR(45) NULL,
                    [TimestampUtc] DATETIME2(7) NOT NULL CONSTRAINT [DF_AuditLogs_TimestampUtc] DEFAULT SYSUTCDATETIME()
                );
                CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityType_EntityId] ON [audit].[AuditLogs] ([EntityType], [EntityId]);
                CREATE NONCLUSTERED INDEX [IX_AuditLogs_TimestampUtc] ON [audit].[AuditLogs] ([TimestampUtc] DESC);
            END;

            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit].[ApprovalWorkflows]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [audit].[ApprovalWorkflows] (
                    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_ApprovalWorkflows] PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
                    [EntityType] NVARCHAR(100) NOT NULL,
                    [EntityId] NVARCHAR(100) NOT NULL,
                    [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ApprovalWorkflows_Status] DEFAULT 'Draft',
                    [RequestedByUserId] NVARCHAR(128) NOT NULL,
                    [ReviewedByUserId] NVARCHAR(128) NULL,
                    [ReviewerComments] NVARCHAR(1000) NULL,
                    [CreatedAtUtc] DATETIME2(7) NOT NULL CONSTRAINT [DF_ApprovalWorkflows_CreatedAtUtc] DEFAULT SYSUTCDATETIME(),
                    [ReviewedAtUtc] DATETIME2(7) NULL
                );
                CREATE UNIQUE NONCLUSTERED INDEX [IX_ApprovalWorkflows_EntityType_EntityId] ON [audit].[ApprovalWorkflows] ([EntityType], [EntityId]);
            END;
            """;

        await dbContext.Database.ExecuteSqlRawAsync(sqlScript);
    }
}