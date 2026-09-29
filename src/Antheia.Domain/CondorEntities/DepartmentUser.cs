using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Domain.CondorEntities
{
    public partial class DepartmentUser
    {
        public int DepartmentId { get; set; }

        public Guid UserId { get; set; }

        public bool IsActive { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid UpdatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }

        public int? RoleId { get; set; }

        public virtual DepartmentRecord Department { get; set; } = null!;
    }
}
