using Antheia.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<UserAuthData?> GetUserAuthDataByUsernameAsync(string username);
    }
}
