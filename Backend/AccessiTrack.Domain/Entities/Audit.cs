using System;
using System.Collections.Generic;
using System.Text;

using AccessiTrack.Domain.Enums;
using AccessiTrack.Domain.Exceptions;

namespace AccessiTrack.Domain.Entities;

public class Audit
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public AuditStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public Project Project { get; private set; } = null!;
    public ICollection<Violation> Violations { get; private set; } = new List<Violation>();

    private Audit() { }

    public static Audit Start(Guid projectId)
    {
        return new Audit
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            StartedAt = DateTime.UtcNow,
            Status = AuditStatus.InProgress
        };
    }

    // Règle métier : ne peut pas être complété avec des violations critiques ouvertes
    public void Complete()
    {
        bool hasOpenCritical = Violations.Any(v =>
            v.Severity == ViolationSeverity.Critical && !v.IsResolved);

        if (hasOpenCritical)
            throw new DomainException(
                "Impossible de terminer l'audit : des violations critiques sont encore ouvertes.");

        Status = AuditStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}
