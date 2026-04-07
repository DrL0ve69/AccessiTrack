using MediatR;

namespace AccessiTrack.Application.Audits.Commands.StartAutomaticAudit;

/// <summary>
/// Command to start an automatic accessibility audit.
/// Fetches the project's target URL and scans for WCAG violations.
/// </summary>
public record StartAutomaticAuditCommand(Guid ProjectId) : IRequest<StartAutomaticAuditResult>;

/// <summary>
/// Result of starting an automatic audit, includes the audit ID and violations found.
/// </summary>
public record StartAutomaticAuditResult(
    Guid AuditId,
    int ViolationsFound,
    int CriticalViolations,
    int MajorViolations,
    int MinorViolations
);
