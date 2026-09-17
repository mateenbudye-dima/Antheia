using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Infrastructure.Entities
{
    public class AspnetRole
    {
        public Guid ApplicationId { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public string LoweredRoleName { get; set; } = null!;

        // Navigation Property
        public ICollection<AspnetUser> Users { get; set; } = new List<AspnetUser>();
    }
}
