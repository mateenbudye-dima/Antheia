using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Antheia.Infrastructure.Repositories;
using Antheia.Infrastructure.Security;
using Microsoft.Extensions.Logging;

namespace Antheia.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto request)
    {
        try
        {
            _logger.LogInformation("AuthenticateAsync called for username {Username}", request.Username);

            var user = await _userRepository.GetUserAuthDataByUsernameAsync(request.Username);

            // Verify membership status
            if (user == null)
            {
                _logger.LogWarning("AuthenticateAsync: user {Username} not found", request.Username);
                return null;
            }

            if (!user.IsApproved || user.IsLockedOut)
            {
                _logger.LogWarning("AuthenticateAsync: user {Username} not approved or locked out (Approved:{Approved} Locked:{Locked})", request.Username, user.IsApproved, user.IsLockedOut);
                return null;
            }

            // Verify legacy hashed password
            bool isValid = LegacyPasswordHasher.VerifyPassword(
                request.Password,
                user.Password,
                user.PasswordSalt,
                user.PasswordFormat
            );

            if (!isValid)
            {
                _logger.LogWarning("AuthenticateAsync: invalid password for user {Username}", request.Username);
                return null;
            }

            var token = _jwtTokenGenerator.GenerateToken(user);
            _logger.LogInformation("AuthenticateAsync: authentication successful for user {Username}", request.Username);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuthenticateAsync failed for username {Username}", request?.Username);
            throw;
        }
    }
}