using Antheia.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Antheia.Domain.Entities;

public partial class BlendRecord
{
    public int BlendId { get; set; }

    public short OrganizationId { get; set; }

    public bool? IsPublished { get; set; }

    public string Title { get; set; } = null!;

    public string? Objective { get; set; }

    public string? Description { get; set; }

    public Guid AuthorId { get; set; }

    public bool IsActive { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid UpdatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public BlendStatus? Status { get; set; }

    public bool? IsMarkedComplete { get; set; }

    public string? BlendPrefix { get; set; }

    public int? RunningNumber { get; set; }
}
