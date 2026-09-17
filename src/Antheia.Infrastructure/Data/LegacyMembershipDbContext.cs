using Antheia.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        }
    }
}
