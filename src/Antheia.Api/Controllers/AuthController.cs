using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Antheia.Api.Controllers;

[ApiController]
[ApiVersion("1.0")] // Defines this controller as v1.0
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var authResponse = await _authService.AuthenticateAsync(request);

        if (authResponse == null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        return Ok(authResponse);
    }
}