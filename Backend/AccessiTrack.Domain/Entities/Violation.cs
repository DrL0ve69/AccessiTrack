using System;
using System.Collections.Generic;
using System.Text;

using AccessiTrack.Domain.Enums;

namespace AccessiTrack.Domain.Entities;

public class Violation
{
    public Guid Id { get; private set; }
    public Guid AuditId { get; private set; }
    public string WcagCriterion { get; private set; } = string.Empty; // ex: "1.4.3"
    public string WcagCriterionName { get; private set; } = string.Empty; // ex: "Contrast (Minimum)"
    public string HtmlElement { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ViolationSeverity Severity { get; private set; }
    public bool IsResolved { get; private set; }
    public string? ResolutionNote { get; private set; }
    public DateTime ReportedAt { get; private set; }

    public Audit Audit { get; private set; } = null!;

    private Violation() { }

    public static Violation Report(
        Guid auditId,
        string wcagCriterion,
        string wcagCriterionName,
        string htmlElement,
        string description,
        ViolationSeverity severity)
    {
        return new Violation
        {
            Id = Guid.NewGuid(),
            AuditId = auditId,
            WcagCriterion = wcagCriterion,
            WcagCriterionName = wcagCriterionName,
            HtmlElement = htmlElement,
            Description = description,
            Severity = severity,
            IsResolved = false,
            ReportedAt = DateTime.UtcNow
        };
    }

    public void MarkAsResolved(string resolutionNote)
    {
        IsResolved = true;
        ResolutionNote = resolutionNote;
    }
}
