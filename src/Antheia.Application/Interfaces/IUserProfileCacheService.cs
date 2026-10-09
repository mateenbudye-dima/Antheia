using Antheia.Application.DTOs;
using System;

namespace Antheia.Application.Interfaces;

public interface IUserProfileCacheService
{
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
    Task SetUserProfileAsync(Guid userId, UserProfileDto profile, TimeSpan? ttl = null);
    Task<UserProfileDto?> GetOrSetUserProfileAsync(Guid userId, Func<Task<UserProfileDto?>> factory, TimeSpan? ttl = null);
    Task RemoveUserProfileAsync(Guid userId);
}
