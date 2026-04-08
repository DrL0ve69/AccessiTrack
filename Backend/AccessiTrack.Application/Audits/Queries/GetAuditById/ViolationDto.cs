namespace AccessiTrack.Application.Audits.Queries.GetAuditById;

public record ViolationDto(
    Guid Id,
    Guid AuditId,
    string WcagCriterion,
    string WcagCriterionName,
    string HtmlElement,
    string Description,
    string Severity,
    bool IsResolved,
    string? ResolutionNote,
    DateTime ReportedAt
);
