using System;
using System.Collections.Generic;

namespace Antheia.Domain.Entities;

public partial class PreparationMethod
{
    public int PreparationId { get; set; }

    public int SectionId { get; set; }

    public string? AdditionSequence { get; set; }

    public string? MixingSpeed { get; set; }

    public string? MixingTime { get; set; }

    public string? Temperature { get; set; }

    public bool IsActive { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid UpdatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual SectionRecord Section { get; set; } = null!;
}
