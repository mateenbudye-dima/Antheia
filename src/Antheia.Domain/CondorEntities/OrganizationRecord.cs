using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Domain.CondorEntities
{
    public partial class OrganizationRecord
    {
        public short OrganizationId { get; set; }

        public string OrganizationCode { get; set; } = null!;

        public string OrganizationName { get; set; } = null!;

        public string? Website { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Fax { get; set; }

        public string? Logo { get; set; }

        public byte? PasswordChangeDays { get; set; }

        public DateTime? BackupStartTime { get; set; }

        public DateTime? LastSuccessfulBackupDate { get; set; }

        public Guid? AdministratorId { get; set; }

        public bool IsActive { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid UpdatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string? WorkflowFileUrl { get; set; }

        public string? AccessCode { get; set; }

        public DateTime? AccessValidTill { get; set; }

        public string? Gstnumber { get; set; }

        public bool? ShowHelpVideos { get; set; }

        public int? PasswordExpiryNotification { get; set; }

        public virtual ICollection<DepartmentRecord> Departments { get; set; } = new List<DepartmentRecord>();
    }
}
