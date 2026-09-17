using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Antheia.Infrastructure.Repositories;
using Antheia.Infrastructure.Security;

namespace Antheia.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetUserAuthDataByUsernameAsync(request.Username);

        // Verify membership status
        if (user == null || !user.IsApproved || user.IsLockedOut)
        {
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
            return null;
        }

        return _jwtTokenGenerator.GenerateToken(user);
    }
}