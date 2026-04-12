using AccessiTrack.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Audits.DTOs;

public record AuditSummaryDto(
    Guid Id,
    AuditStatus Status,
    int? Score,
    int ViolationCount,
    int PassCount,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record AuditResultDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Url,
    AuditStatus Status,
    int? Score,
    int ViolationCount,
    int PassCount,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    IReadOnlyList<ViolationDto> Violations
);

public record ViolationDto(
    Guid Id,
    string RuleId,
    string Description,
    ViolationSeverity Severity,
    string WcagCriteria,
    string HelpUrl,
    string HtmlElement,
    string Selector,
    string FailureSummary
);
