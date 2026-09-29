using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Domain.CondorEntities
{
    public partial class DepartmentRecord
    {
        public int DepartmentId { get; set; }

        public short OrganizationId { get; set; }

        public string? DepartmentCode { get; set; }

        public string DepartmentName { get; set; } = null!;

        public byte DepartmentType { get; set; }

        public Guid? DepartmentHead { get; set; }

        public bool? AllowPrintingExperiments { get; set; }

        public bool? AllowSharingExperiments { get; set; }

        public bool? AllowExperimentLayoutModfication { get; set; }

        public byte? LimsSetting { get; set; }

        public int? ExperimentNumberSelector { get; set; }

        public bool? HideWitnessingDatetime { get; set; }

        public bool? DisplayExperimentLevelPrecautions { get; set; }

        public string? ConclusionLabel { get; set; }

        public string? ExperimentLabel { get; set; }

        public bool? AllowProjectDelete { get; set; }

        public bool? AllowPublicTemplates { get; set; }

        public bool? AllowReactionTimeSelect { get; set; }

        public bool? ProjectStagesAreMandatory { get; set; }

        public bool? AllowSelfSigning { get; set; }

        public bool IsActive { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid UpdatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }

        public bool? ValidateStockAvailability { get; set; }

        public bool? CanViewResourceCostGl { get; set; }

        public bool? CanViewAnalysisCostGl { get; set; }

        public bool? CanViewMaterialCostGl { get; set; }

        public bool? CanViewUserPerformanceGl { get; set; }

        public bool? CanManageDossierGl { get; set; }

        public bool? CanPrintGl { get; set; }

        public bool? CanManageReviewerApproverGl { get; set; }

        public bool? CanViewResourceCostPl { get; set; }

        public bool? CanViewAnalysisCostPl { get; set; }

        public bool? CanViewMaterialCostPl { get; set; }

        public bool? CanViewUserPerformancePl { get; set; }

        public bool? CanPrintPl { get; set; }

        public bool? CanManageDossierPl { get; set; }

        public bool? CanManageReviewerApproverPl { get; set; }

        public bool? FollowSampleAssignment { get; set; }

        public bool? FollowAbandonApproval { get; set; }

        public bool? CanAbandonExperimentGl { get; set; }

        public bool? CanAbandonExperimentPl { get; set; }

        public bool? AllowStockTestingByChemists { get; set; }

        public bool? AllowAutoSendStabilityTests { get; set; }

        public string? Logo { get; set; }

        public string? Address { get; set; }

        public byte? DivisionType { get; set; }

        public bool? IsVirtualDepartment { get; set; }

        public bool? CanDownloadReportGl { get; set; }

        public bool? CanDownloadReportPl { get; set; }

        public byte? MethodWorkflow { get; set; }

        public bool? AllowAutoSendPreformulationTests { get; set; }

        public int? ExternalStandardDepartmentId { get; set; }

        public string? CategoryCode { get; set; }

        public virtual OrganizationRecord Organization { get; set; } = null!;

        public virtual ICollection<DepartmentUser> Users { get; set; } = new List<DepartmentUser>();
    }
}
