using Antheia.Domain.CondorEntities;
using Antheia.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography.Pkcs;
using System.Text;

namespace Antheia.Infrastructure.Data
{
    public class LegacyMembershipDbContext : DbContext
    {
        public LegacyMembershipDbContext(DbContextOptions<LegacyMembershipDbContext> options)
            : base(options) { }

        public DbSet<AspnetUser> Users => Set<AspnetUser>();
        public DbSet<AspnetMembership> Memberships => Set<AspnetMembership>();
        public DbSet<AspnetRole> Roles => Set<AspnetRole>();

        public virtual DbSet<DepartmentRecord> Departments { get; set; }

        public virtual DbSet<OrganizationRecord> Organizations { get; set; }

        public virtual DbSet<OrganizationRole> OrganizationRoles { get; set; }

        public virtual DbSet<RolePrivilege> RolePrivileges { get; set; }

        public virtual DbSet<DepartmentUser> DepartmentUsers { get; set; }

        public virtual DbSet<ContactInfo> ContactInfos { get; set; }

        public virtual DbSet<UserContact> UserContacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map aspnet_Users
            modelBuilder.Entity<AspnetUser>(entity =>
            {
                entity.ToTable("aspnet_Users", "dbo");
                entity.HasKey(e => e.UserId);

                entity.HasOne(u => u.Membership)
                      .WithOne(m => m.User)
                      .HasForeignKey<AspnetMembership>(m => m.UserId);
            });

            // Map aspnet_Membership
            modelBuilder.Entity<AspnetMembership>(entity =>
            {
                entity.ToTable("aspnet_Membership", "dbo");
                entity.HasKey(e => e.UserId);
            });

            // Map aspnet_Roles and explicit Many-to-Many via aspnet_UsersInRoles
            modelBuilder.Entity<AspnetRole>(entity =>
            {
                entity.ToTable("aspnet_Roles", "dbo");
                entity.HasKey(e => e.RoleId);

                entity.HasMany(r => r.Users)
                      .WithMany(u => u.Roles)
                      .UsingEntity<Dictionary<string, object>>(
                          "aspnet_UsersInRoles",
                          j => j.HasOne<AspnetUser>()
                                .WithMany()
                                .HasForeignKey("UserId")
                                .HasConstraintName("FK__aspnet_Us__UserI__..."),
                          j => j.HasOne<AspnetRole>()
                                .WithMany()
                                .HasForeignKey("RoleId")
                                .HasConstraintName("FK__aspnet_Us__RoleI__..."),
                          j =>
                          {
                              j.ToTable("aspnet_UsersInRoles", "dbo");
                              j.HasKey("UserId", "RoleId");
                          });
            });

            modelBuilder.Entity<DepartmentRecord>(entity =>
            {
                entity.HasKey(e => e.DepartmentId).HasName("PK_Departments");

                entity.ToTable("Record", "Department");

                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.CanAbandonExperimentGl).HasColumnName("CanAbandonExperiment_GL");
                entity.Property(e => e.CanAbandonExperimentPl).HasColumnName("CanAbandonExperiment_PL");
                entity.Property(e => e.CanDownloadReportGl).HasColumnName("CanDownloadReport_GL");
                entity.Property(e => e.CanDownloadReportPl).HasColumnName("CanDownloadReport_PL");
                entity.Property(e => e.CanManageDossierGl).HasColumnName("CanManageDossier_GL");
                entity.Property(e => e.CanManageDossierPl).HasColumnName("CanManageDossier_PL");
                entity.Property(e => e.CanManageReviewerApproverGl).HasColumnName("CanManageReviewerApprover_GL");
                entity.Property(e => e.CanManageReviewerApproverPl).HasColumnName("CanManageReviewerApprover_PL");
                entity.Property(e => e.CanPrintGl).HasColumnName("CanPrint_GL");
                entity.Property(e => e.CanPrintPl).HasColumnName("CanPrint_PL");
                entity.Property(e => e.CanViewAnalysisCostGl).HasColumnName("CanViewAnalysisCost_GL");
                entity.Property(e => e.CanViewAnalysisCostPl).HasColumnName("CanViewAnalysisCost_PL");
                entity.Property(e => e.CanViewMaterialCostGl).HasColumnName("CanViewMaterialCost_GL");
                entity.Property(e => e.CanViewMaterialCostPl).HasColumnName("CanViewMaterialCost_PL");
                entity.Property(e => e.CanViewResourceCostGl).HasColumnName("CanViewResourceCost_GL");
                entity.Property(e => e.CanViewResourceCostPl).HasColumnName("CanViewResourceCost_PL");
                entity.Property(e => e.CanViewUserPerformanceGl).HasColumnName("CanViewUserPerformance_GL");
                entity.Property(e => e.CanViewUserPerformancePl).HasColumnName("CanViewUserPerformance_PL");
                entity.Property(e => e.CategoryCode)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.ConclusionLabel)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
                entity.Property(e => e.DepartmentCode)
                    .HasMaxLength(25)
                    .IsUnicode(false);
                entity.Property(e => e.DepartmentName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.ExperimentLabel)
                    .HasMaxLength(15)
                    .IsUnicode(false);
                entity.Property(e => e.Logo)
                    .HasMaxLength(250)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");

                entity.HasOne(d => d.Organization).WithMany(p => p.Departments)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Departments_Organization");
            });

            modelBuilder.Entity<OrganizationRecord>(entity =>
            {
                entity.HasKey(e => e.OrganizationId).HasName("PK_Record_1");

                entity.ToTable("Record", "Organization");

                entity.Property(e => e.AccessCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.AccessValidTill).HasColumnType("datetime");
                entity.Property(e => e.BackupStartTime).HasColumnType("smalldatetime");
                entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Fax)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Gstnumber)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("GSTNumber");
                entity.Property(e => e.LastSuccessfulBackupDate).HasColumnType("smalldatetime");
                entity.Property(e => e.Logo)
                    .HasMaxLength(250)
                    .IsUnicode(false);
                entity.Property(e => e.OrganizationCode)
                    .HasMaxLength(15)
                    .IsUnicode(false);
                entity.Property(e => e.OrganizationName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");
                entity.Property(e => e.Website)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.WorkflowFileUrl)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("WorkflowFileURL");
            });

            modelBuilder.Entity<OrganizationRole>(entity =>
            {
                entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A3D39D4E1");

                entity.ToTable("Roles", "Organization");

                entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
                entity.Property(e => e.RoleName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");
            });

            modelBuilder.Entity<RolePrivilege>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PrivilegeId });

                entity.ToTable("RolePrivileges", "Organization");

                entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
                entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");

                entity.HasOne(d => d.Role).WithMany(p => p.RolePrivileges)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RolePrivileges_OrganizationRole");
            });

            modelBuilder.Entity<DepartmentUser>(entity =>
            {
                entity.HasKey(e => new { e.DepartmentId, e.UserId });

                entity.ToTable("Users", "Department");

                entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
                entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");

                entity.HasOne(d => d.Department).WithMany(p => p.Users)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Users_DepartmentRecord");
            });

            modelBuilder.Entity<ContactInfo>(entity =>
            {
                entity.HasKey(e => e.ContactId).HasName("PK_Contact_ContactID");

                entity.ToTable("ContactInfo", "Person");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(250)
                    .IsUnicode(false);
                entity.Property(e => e.BirthDate).HasColumnType("smalldatetime");
                entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.ExperienceSpecifiedOn).HasColumnType("smalldatetime");
                entity.Property(e => e.Fax)
                    .HasMaxLength(25)
                    .IsUnicode(false);
                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.HomePhone)
                    .HasMaxLength(25)
                    .IsUnicode(false);
                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(250)
                    .IsUnicode(false);
                entity.Property(e => e.IsActive)
                    .HasComment("0=N 1=Y")
                    .HasDefaultValue(true, "DF_Contact_IsActive");
                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.MaritalStatusId).HasDefaultValue((byte)0, "DF_Contact_MaritalStatus");
                entity.Property(e => e.MiddleName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.MobilePhone)
                    .HasMaxLength(25)
                    .IsUnicode(false);
                entity.Property(e => e.Qualification)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.SkypeId)
                    .HasMaxLength(25)
                    .IsUnicode(false);
                entity.Property(e => e.Suffix)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.Title)
                    .HasMaxLength(8)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");
                entity.Property(e => e.WeChatId)
                    .HasMaxLength(25)
                    .IsUnicode(false);
                entity.Property(e => e.WhatsappNumber)
                    .HasMaxLength(25)
                    .IsUnicode(false);
                entity.Property(e => e.WorkPhone)
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserContact>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ContactId });

                entity.ToTable("Record", "Person");

                entity.Property(e => e.AccessCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.AccessCodeExpirydate).HasColumnType("datetime");
                entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
                entity.Property(e => e.LoginUserIp)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("LoginUserIP");
                entity.Property(e => e.PasswordResetOn).HasColumnType("smalldatetime");
                entity.Property(e => e.Status).HasMaxLength(1000);
                entity.Property(e => e.UpdatedDate).HasColumnType("smalldatetime");

                entity.HasOne(d => d.Contact).WithMany(p => p.UserContacts)
                    .HasForeignKey(d => d.ContactId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Record_ContactInfo");
            });

        }
    }
}
