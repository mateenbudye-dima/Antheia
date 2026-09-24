using Dima.WorkflowAuditing.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dima.WorkflowAuditing.Data;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ApprovalWorkflow> ApprovalWorkflows => Set<ApprovalWorkflow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("AuditLogs", "audit");
            b.HasKey(x => x.Id);
            b.Property(x => x.TimestampUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<ApprovalWorkflow>(b =>
        {
            b.ToTable("ApprovalWorkflows", "audit");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.EntityType, x.EntityId }).IsUnique();
            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}