using Dima.ChangeAudit.Models.Application;
using Dima.ChangeAudit.Models.Display;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dima.ChangeAudit.Abstractions
{
    public interface IAuditLogDisplayFormatter
    {
        FormattedAuditLogResult Format(AuditLogResult item);
        IReadOnlyList<FormattedAuditLogResult> FormatMany(IEnumerable<AuditLogResult> items);
        FormattedAuditLogPagedResult FormatPaged(AuditLogPagedResult pagedResult);
    }
}
