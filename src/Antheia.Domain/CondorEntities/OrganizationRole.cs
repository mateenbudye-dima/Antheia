using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Domain.CondorEntities
{
    public partial class OrganizationRole
    {
        public int RoleId { get; set; }

        public int OrganizationId { get; set; }

        public string RoleName { get; set; } = null!;

        public byte DepartmentType { get; set; }

        public bool IsActive { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid UpdatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }

        public byte? RoleType { get; set; }

        public virtual ICollection<RolePrivilege> RolePrivileges { get; set; } = new List<RolePrivilege>();
    }
}
