using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Domain.Enums;

public enum AuditStatus
{
    Pending = 0,
    InProgress = 1,
    PendingReview = 2,
    Completed = 3,
    Archived = 4,
    Failed = 5
}
