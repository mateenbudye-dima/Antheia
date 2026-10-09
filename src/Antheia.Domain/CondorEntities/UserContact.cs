using System;
using System.Collections.Generic;

namespace Antheia.Domain.CondorEntities;

public partial class UserContact
{
    public Guid UserId { get; set; }

    public int ContactId { get; set; }

    public string? Code { get; set; }

    public string? Status { get; set; }

    public bool? IsFirstTimeUser { get; set; }

    public DateTime? PasswordResetOn { get; set; }

    public bool IsActive { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid UpdatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public bool? IsLoggedIn { get; set; }

    public string? LoginUserIp { get; set; }

    public bool? IsAccessCodeRequired { get; set; }

    public string? AccessCode { get; set; }

    public DateTime? AccessCodeExpirydate { get; set; }

    public bool? IsPasswordResetRquested { get; set; }

    public virtual ContactInfo Contact { get; set; } = null!;
}
