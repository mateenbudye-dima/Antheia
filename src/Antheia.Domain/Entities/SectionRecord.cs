using System;
using System.Collections.Generic;

namespace Antheia.Domain.Entities;

public partial class SectionRecord
{
    public int SectionId { get; set; }

    public int ContainerId { get; set; }

    public byte ContainerTypeId { get; set; }

    public byte? SectionOrder { get; set; }

    public byte SectionTypeId { get; set; }

    public string SectionTitle { get; set; } = null!;

    public bool IsActive { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid UpdatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

    public virtual ICollection<PreparationMethod> PreparationMethods { get; set; } = new List<PreparationMethod>();
}
