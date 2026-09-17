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

            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Membership)
                .Include(u => u.Roles)
                .Where(u => u.LoweredUserName == lowered)
                .Select(u => new UserAuthData(
                    u.UserId,
                    u.UserName,
                    u.Membership!.Password,
                    u.Membership.PasswordSalt,
                    u.Membership.PasswordFormat,
                    u.Membership.IsApproved,
                    u.Membership.IsLockedOut,
                    u.Roles.Select(r => r.RoleName).ToList()
                ))
                .FirstOrDefaultAsync();
        }
    }
}
