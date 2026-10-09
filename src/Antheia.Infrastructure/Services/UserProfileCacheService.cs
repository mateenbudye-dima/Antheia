using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Antheia.Infrastructure.Services;

public class UserCacheOptions
{
    // TTL in seconds
    public int DefaultTtlSeconds { get; set; } = 300;
}

public class UserProfileCacheService : IUserProfileCacheService
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _defaultTtl;

    public UserProfileCacheService(IDistributedCache cache, IOptions<UserCacheOptions> options)
    {
        _cache = cache;
        _defaultTtl = TimeSpan.FromSeconds(options?.Value?.DefaultTtlSeconds ?? 300);
    }

    private static string KeyFor(Guid userId) => $"profile:{userId.ToString("N")}";

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
    {
        var key = KeyFor(userId);
        var data = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(data)) return null;
        return JsonSerializer.Deserialize<UserProfileDto>(data);
    }

    public async Task SetUserProfileAsync(Guid userId, UserProfileDto profile, TimeSpan? ttl = null)
    {
        var key = KeyFor(userId);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
        };
        var json = JsonSerializer.Serialize(profile);
        await _cache.SetStringAsync(key, json, options);
    }

    public async Task<UserProfileDto?> GetOrSetUserProfileAsync(Guid userId, Func<Task<UserProfileDto?>> factory, TimeSpan? ttl = null)
    {
        var cached = await GetUserProfileAsync(userId);
        if (cached != null) return cached;

        var fresh = await factory();
        if (fresh != null)
        {
            await SetUserProfileAsync(userId, fresh, ttl);
        }

        return fresh;
    }

    public async Task RemoveUserProfileAsync(Guid userId)
    {
        var key = KeyFor(userId);
        await _cache.RemoveAsync(key);
    }
}
