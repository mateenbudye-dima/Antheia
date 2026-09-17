using Antheia.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        AuthResponseDto GenerateToken(UserAuthData user);
    }
}
