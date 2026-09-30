using Microsoft.AspNetCore.Http;

namespace Dima.ChangeAudit.Abstractions;

public interface IUserContext
{
    string UserId { get; }
    string UserName { get; }
}

public class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public string UserId =>
        httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
        ?? httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? "ANONYMOUS";

    public string UserName =>
        httpContextAccessor.HttpContext?.User?.Identity?.Name
        ?? "Unknown User";
}