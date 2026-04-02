using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Domain.Enums;

public enum AuditStatus
{
    InProgress = 1,
    PendingReview = 2,
    Completed = 3,
    Archived = 4
}
