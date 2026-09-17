using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.DTOs
{
    public record LoginRequestDto(string Username, string Password);

    public record AuthResponseDto(string Token, string Username, Guid UserId, IEnumerable<string> Roles, DateTime ExpiresAt);

    public record UserAuthData(
        Guid UserId,
        string UserName,
        string Password,
        string PasswordSalt,
        int PasswordFormat,
        bool IsApproved,
        bool IsLockedOut,
        List<string> Roles
    );
}
