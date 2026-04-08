namespace AccessiTrack.Application.Audits.Queries.GetAuditById;

public record AuditDetailsDto(
    Guid Id,
    Guid ProjectId,
    string Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string? Notes,
    int TotalViolations,
    int CriticalViolations,
    int ResolvedViolations,
    IEnumerable<ViolationDto> Violations
);
