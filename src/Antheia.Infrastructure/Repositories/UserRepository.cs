using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
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
        private readonly IUserProfileCacheService _profileCache;

        public UserRepository(LegacyMembershipDbContext context, IUserProfileCacheService profileCache)
        {
            _context = context;
            _profileCache = profileCache;
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

                // 1. Left Join Roles
                join role in _context.OrganizationRoles on du.RoleId equals role.RoleId into roleGroup
                from role in roleGroup.DefaultIfEmpty()

                    // 2. Left Join RolePrivileges (Join on nullable role.RoleId)
                join rp in _context.RolePrivileges on (role != null ? (long?)role.RoleId : null) equals rp.RoleId into rpGroup
                from rp in rpGroup.DefaultIfEmpty()

                where du.UserId == userBase.UserId
                    && du.IsActive
                    && dept.IsActive
                    // Crucial: allow NULLs through for optional left joins
                    && (role == null || role.IsActive)
                    && (rp == null || rp.IsActive)
                select new
                {
                    dept.OrganizationId,
                    RoleName = role != null ? role.RoleName : null,
                    PrivilegeId = rp != null ? (short?)rp.PrivilegeId : null
                }
            ).AsNoTracking().ToListAsync();

            // Step 3: Materialize distinct lists in memory
            var roles = rolePrivilegeData
                .Select(x => x.RoleName)
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .ToList();

            var privileges = rolePrivilegeData
                .Where(x => x.PrivilegeId.HasValue)
                .Select(x => (int)x.PrivilegeId!.Value)
                .Distinct()
                .ToList();

            // Attempt to determine OrganizationId from role-joined data first; if not available, fall back to department membership
            int organizationId = rolePrivilegeData.Select(x => (int?)x.OrganizationId).FirstOrDefault() ?? 0;
            if (organizationId == 0)
            {
                var deptOrg = await (
                    from du in _context.DepartmentUsers
                    join dept in _context.Departments on du.DepartmentId equals dept.DepartmentId
                    where du.UserId == userBase.UserId
                       && du.IsActive
                       && dept.IsActive
                    select (int?)dept.OrganizationId
                ).FirstOrDefaultAsync();

                if (deptOrg.HasValue)
                {
                    organizationId = deptOrg.Value;
                }
            }

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
        // New: retrieve lightweight profile info and cache it via IUserProfileCacheService
        public async Task<UserProfileDto?> GetUserProfileByUsernameAsync(string username)
        {
            // First resolve the user to get userId, then use userId as cache key
            var lowered = username.ToLowerInvariant();

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.LoweredUserName == lowered)
                .Select(u => new { u.UserId, u.UserName })
                .FirstOrDefaultAsync();

            if (user == null) return null;

            return await _profileCache.GetOrSetUserProfileAsync(user.UserId, async () => await BuildUserProfileByIdAsync(user.UserId, user.UserName));
        }

        private async Task<UserProfileDto?> BuildUserProfileByIdAsync(Guid userId, string userName)
        {
            // Attempt to find department id, name, organization id and name for the user by userId
            var deptInfo = await (
                from du in _context.DepartmentUsers
                join d in _context.Departments on du.DepartmentId equals d.DepartmentId
                join org in _context.Organizations on d.OrganizationId equals org.OrganizationId
                where du.UserId == userId && du.IsActive && d.IsActive && org.IsActive
                select new
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    OrganizationId = d.OrganizationId,
                    OrganizationName = org.OrganizationName
                }
            ).FirstOrDefaultAsync();

            // Attempt to build display name from contact info
            var displayName = await (
                from uc in _context.UserContacts
                join ci in _context.ContactInfos on uc.ContactId equals ci.ContactId
                where uc.UserId == userId && uc.IsActive
                select (ci.FirstName + " " + ci.LastName)
            ).FirstOrDefaultAsync();

            var deptName = deptInfo?.DepartmentName;
            var deptId = deptInfo?.DepartmentId;
            var orgId = deptInfo?.OrganizationId ?? 0;
            var orgName = deptInfo?.OrganizationName;

            return new UserProfileDto(userId, userName, displayName ?? userName, deptId, deptName, orgId, orgName);
        }

        
    }
}
