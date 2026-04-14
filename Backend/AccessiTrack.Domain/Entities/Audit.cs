using System;
using System.Collections.Generic;
using System.Text;

using AccessiTrack.Domain.Enums;
using AccessiTrack.Domain.Exceptions;

namespace AccessiTrack.Domain.Entities;

public class Audit
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProjectId { get; private set; }
    public AuditStatus Status { get; private set; } = AuditStatus.Pending;
    public int? Score { get; private set; }
    public int ViolationCount { get; private set; }
    public int PassCount { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public Project Project { get; private set; } = null!;
    public ICollection<Violation> Violations { get; private set; } = [];

    // Factory
    public static Audit Create(Guid projectId) => new() { ProjectId = projectId };

    // State machine
    public void MarkInProgress()
    {
        Status = AuditStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(int score, int violationCount, int passCount)
    {
        Status = AuditStatus.Completed;
        Score = score;
        ViolationCount = violationCount;
        PassCount = passCount;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = AuditStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }
}
