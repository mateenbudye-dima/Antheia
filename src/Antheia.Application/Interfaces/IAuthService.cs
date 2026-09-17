using Antheia.Application.DTOs;

namespace Antheia.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto request);
    }
}
