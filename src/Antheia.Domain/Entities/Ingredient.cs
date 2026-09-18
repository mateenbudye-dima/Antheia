using System;
using System.Collections.Generic;

namespace Antheia.Domain.Entities;

public partial class Ingredient
{
    public int SectionIngredientId { get; set; }

    public int SectionId { get; set; }

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public decimal? Ratio { get; set; }

    public decimal? Quantity { get; set; }

    public bool IsActive { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid UpdatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual SectionRecord Section { get; set; } = null!;
}
