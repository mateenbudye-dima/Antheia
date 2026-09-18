using System.Security.Claims;
using Antheia.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Antheia.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }   

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user?.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("User identifier claim is missing or invalid.");
        }
    }

    public short OrganizationId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var orgClaim = user?.FindFirst("OrganizationId")?.Value
                        ?? user?.FindFirst("org_id")?.Value;

            return short.TryParse(orgClaim, out var orgId)
                ? orgId
                : (short)1; // Default fallback organization
        }
    }
}