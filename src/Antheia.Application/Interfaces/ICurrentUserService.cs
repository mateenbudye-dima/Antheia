using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    short OrganizationId { get; }
}