using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Audits.Queries.GetAuditsByProject;

public record AuditDto(
    Guid Id,
    Guid ProjectId,
    string Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int TotalViolations,
    int CriticalViolations,
    int ResolvedViolations
);
