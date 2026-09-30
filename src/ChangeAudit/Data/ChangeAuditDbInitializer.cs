using Microsoft.EntityFrameworkCore;

namespace Dima.ChangeAudit.Data;

public static class ChangeAuditDbInitializer
{
    public static async Task InitializeAsync(ChangeAuditDbContext dbContext, CancellationToken cancellationToken = default)
    {
        const string sqlScript = """
            IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'ChangeAudit')
            BEGIN
                EXEC('CREATE SCHEMA [ChangeAudit] AUTHORIZATION [dbo]');
            END;

            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ChangeAudit].[AuditChangeLogs]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [ChangeAudit].[AuditChangeLogs] (
                    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_AuditChangeLogs] PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
                    [EntityType] NVARCHAR(100) NOT NULL,
                    [EntityId] NVARCHAR(100) NOT NULL,
                    [Action] NVARCHAR(20) NOT NULL,
                    [UserId] UNIQUEIDENTIFIER NOT NULL,
                    [TimestampUtc] DATETIME2(7) NOT NULL CONSTRAINT [DF_AuditChangeLogs_TimestampUtc] DEFAULT SYSUTCDATETIME()
                );

                CREATE NONCLUSTERED INDEX [IX_AuditChangeLogs_EntityType_EntityId] 
                    ON [ChangeAudit].[AuditChangeLogs] ([EntityType], [EntityId]);

                CREATE NONCLUSTERED INDEX [IX_AuditChangeLogs_TimestampUtc] 
                    ON [ChangeAudit].[AuditChangeLogs] ([TimestampUtc] DESC);
            END;

            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ChangeAudit].[AuditLogDetails]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [ChangeAudit].[AuditLogDetails] (
                    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_AuditLogDetails] PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
                    [AuditChangeLogId] UNIQUEIDENTIFIER NOT NULL,
                    [PropertyName] NVARCHAR(100) NOT NULL,
                    [OldValue] NVARCHAR(MAX) NULL,
                    [NewValue] NVARCHAR(MAX) NULL,
                    CONSTRAINT [FK_AuditLogDetails_AuditChangeLogs] FOREIGN KEY ([AuditChangeLogId]) 
                        REFERENCES [ChangeAudit].[AuditChangeLogs] ([Id]) ON DELETE CASCADE
                );

                CREATE NONCLUSTERED INDEX [IX_AuditLogDetails_AuditChangeLogId] 
                    ON [ChangeAudit].[AuditLogDetails] ([AuditChangeLogId]);
            END;
            """;

        await dbContext.Database.ExecuteSqlRawAsync(sqlScript, cancellationToken);
    }
}