using Dima.ChangeAudit.Attributes;
using System;
using System.Collections.Generic;

namespace Antheia.Domain.Entities;

[Auditable]
public partial class Evaluation
{
    public int EvaluationId { get; set; }

    public int SectionId { get; set; }

    public string EvaluationParameter { get; set; } = " ";

    public string? Result { get; set; }

    public string? Specification { get; set; }

    public string? Status { get; set; }

    public bool IsActive { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    [IgnoreAudit]
    public Guid UpdatedBy { get; set; }

    [IgnoreAudit]
    public DateTime UpdatedDate { get; set; }

    public virtual SectionRecord Section { get; set; } = null!;
}
