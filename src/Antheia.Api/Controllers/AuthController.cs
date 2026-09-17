using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Antheia.Infrastructure.Repositories;
using Antheia.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Antheia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthController(ILogger<AuthController> logger, IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _logger = logger;
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _userRepository.GetUserAuthDataByUsernameAsync(request.Username);

            if (user == null || !user.IsApproved || user.IsLockedOut)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            bool isValid = LegacyPasswordHasher.VerifyPassword(
                request.Password,
                user.Password,
                user.PasswordSalt,
                user.PasswordFormat
            );

            if (!isValid)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var response = _jwtTokenGenerator.GenerateToken(user);
            return Ok(response);
        }
    }
}
