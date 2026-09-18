using Antheia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Antheia.Infrastructure.Data;

public partial class AntheiaDbContext : DbContext
{
    public AntheiaDbContext(DbContextOptions<AntheiaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Evaluation> Evaluations { get; set; }
    public virtual DbSet<Ingredient> Ingredients { get; set; }
    public virtual DbSet<PreparationMethod> PreparationMethods { get; set; }
    public virtual DbSet<SectionRecord> SectionRecords { get; set; }
    public virtual DbSet<TemplateRecord> TemplateRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Evaluation>(entity =>
        {
            entity.ToTable("Evaluation", "Section");

            entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.Result).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Specification).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");

            entity.HasOne(d => d.Section)
                .WithMany(p => p.Evaluations)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Evaluation_Record");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.SectionIngredientId);
            entity.ToTable("Ingredients", "Section");

            entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.Name).HasMaxLength(200).IsUnicode(false);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.Ratio).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.Type).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");

            entity.HasOne(d => d.Section)
                .WithMany(p => p.Ingredients)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingredients_Record");
        });

        modelBuilder.Entity<PreparationMethod>(entity =>
        {
            entity.HasKey(e => e.PreparationId);
            entity.ToTable("PreparationMethod", "Section");

            entity.Property(e => e.AdditionSequence).HasMaxLength(2000).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.MixingSpeed).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.MixingTime).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Temperature).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");

            entity.HasOne(d => d.Section)
                .WithMany(p => p.PreparationMethods)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PreparationMethod_Record");
        });

        modelBuilder.Entity<SectionRecord>(entity =>
        {
            entity.HasKey(e => e.SectionId);
            entity.ToTable("Record", "Section");

            entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.SectionTitle).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");
        });

        modelBuilder.Entity<TemplateRecord>(entity =>
        {
            // Fixed: Explicit Primary Key mapping
            entity.HasKey(e => e.TemplateId);
            entity.ToTable("Record", "Template");

            entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.TemplatePrefix).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(200).IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}