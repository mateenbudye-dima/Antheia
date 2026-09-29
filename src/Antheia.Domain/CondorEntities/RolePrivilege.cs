using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Antheia.Domain.CondorEntities
{
    public partial class RolePrivilege
    {
        public int RoleId { get; set; }

        public short PrivilegeId { get; set; }

        public bool IsActive { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid UpdatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }

        public virtual OrganizationRole Role { get; set; } = null!;
    }
}
