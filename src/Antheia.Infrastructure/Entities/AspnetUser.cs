using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Infrastructure.Entities
{
    public class AspnetUser
    {
        public Guid UserId { get; set; }
        public Guid ApplicationId { get; set; }
        public string UserName { get; set; } = null!;
        public string LoweredUserName { get; set; } = null!;
        public string? MobileAlias { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime LastActivityDate { get; set; }

        // Navigation Property
        public AspnetMembership? Membership { get; set; }
        public ICollection<AspnetRole> Roles { get; set; } = new List<AspnetRole>();
    }
}
