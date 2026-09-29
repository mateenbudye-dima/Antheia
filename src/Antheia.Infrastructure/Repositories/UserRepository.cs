using Antheia.Application.DTOs;
using Antheia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LegacyMembershipDbContext _context;

        public UserRepository(LegacyMembershipDbContext context)
        {
            _context = context;
        }

        public async Task<UserAuthData?> GetUserAuthDataByUsernameAsync(string username)
        {
            var lowered = username.ToLowerInvariant();

            // Step 1: Query basic user authentication info
            var userBase = await _context.Users
                .AsNoTracking()
                .Where(u => u.LoweredUserName == lowered)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    Password = u.Membership!.Password,
                    PasswordSalt = u.Membership.PasswordSalt,
                    PasswordFormat = u.Membership.PasswordFormat,
                    IsApproved = u.Membership.IsApproved,
                    IsLockedOut = u.Membership.IsLockedOut
                })
                .FirstOrDefaultAsync();

            if (userBase == null)
            {
                return null;
            }

            // Step 2: Query OrganizationId, active Roles, and active PrivilegeIds
            var rolePrivilegeData = await (
                from du in _context.DepartmentUsers
                join dept in _context.Departments on du.DepartmentId equals dept.DepartmentId
                join role in _context.OrganizationRoles on du.RoleId equals role.RoleId
                join rp in _context.RolePrivileges on role.RoleId equals rp.RoleId into rpGroup
                from rp in rpGroup.DefaultIfEmpty()
                where du.UserId == userBase.UserId
                   && du.IsActive
                   && dept.IsActive
                   && role.IsActive
                   && (rp == null || rp.IsActive)
                select new
                {
                    dept.OrganizationId,
                    role.RoleName,
                    PrivilegeId = (short?)rp.PrivilegeId
                }
            ).AsNoTracking().ToListAsync();

            // Step 3: Materialize distinct lists in memory
            int organizationId = rolePrivilegeData.Select(x => (int)x.OrganizationId).FirstOrDefault();

            var roles = rolePrivilegeData
                .Select(x => x.RoleName)
                .Distinct()
                .ToList();

            var privileges = rolePrivilegeData
                .Where(x => x.PrivilegeId.HasValue)
                .Select(x => (int)x.PrivilegeId!.Value)
                .Distinct()
                .ToList();

            // Step 4: Construct final DTO
            return new UserAuthData(
                userBase.UserId,
                userBase.UserName,
                userBase.Password,
                userBase.PasswordSalt,
                userBase.PasswordFormat,
                userBase.IsApproved,
                userBase.IsLockedOut,
                organizationId,
                roles,
                privileges
            );
        }
    }
}
