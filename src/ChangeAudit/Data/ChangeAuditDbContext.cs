using Dima.ChangeAudit.Models;
using Microsoft.EntityFrameworkCore;

namespace Dima.ChangeAudit.Data;

public class ChangeAuditDbContext(DbContextOptions<ChangeAuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditChangeLog> AuditChangeLogs => Set<AuditChangeLog>();
    public DbSet<AuditLogDetail> AuditLogDetails => Set<AuditLogDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configure AuditChangeLogs Entity
        modelBuilder.Entity<AuditChangeLog>(entity =>
        {
            entity.ToTable("AuditChangeLogs", schema: "ChangeAudit");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.Property(e => e.EntityType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.EntityId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Action)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            //entity.Property(e => e.UserRoles)
            //    .HasMaxLength(500);

            //entity.Property(e => e.Path)
            //    .HasMaxLength(500);

            //entity.Property(e => e.HttpMethod)
            //    .HasMaxLength(10);

            //entity.Property(e => e.IpAddress)
            //    .HasMaxLength(45);

            entity.Property(e => e.TimestampUtc)
                .HasPrecision(7)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // Non-Clustered Indexes
            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "IX_AuditChangeLogs_EntityType_EntityId");
            entity.HasIndex(e => e.TimestampUtc, "IX_AuditChangeLogs_TimestampUtc").IsDescending();

            // One-to-Many Relationship with Cascade Delete
            entity.HasMany(e => e.Details)
                .WithOne(d => d.AuditChangeLog)
                .HasForeignKey(d => d.AuditChangeLogId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 2. Configure AuditLogDetails Entity
        modelBuilder.Entity<AuditLogDetail>(entity =>
        {
            entity.ToTable("AuditLogDetails", schema: "ChangeAudit");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.Property(e => e.PropertyName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.OldValue)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.NewValue)
                .HasColumnType("nvarchar(max)");

            // Non-Clustered Index
            entity.HasIndex(e => e.AuditChangeLogId, "IX_AuditLogDetails_AuditChangeLogId");
        });
    }
}