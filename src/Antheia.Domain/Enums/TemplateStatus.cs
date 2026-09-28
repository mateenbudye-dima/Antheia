using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Domain.Enums
{
    public enum TemplateStatus:byte
    {
        Draft = 0,
        SubmittedForReview = 1,
        Reviewed = 2,
        SubmittedForApproval = 3,
        Approved = 4,
        Rejected = 5
    }
}
