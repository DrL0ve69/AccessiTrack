using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Common.Interfaces;

public record AuditJob(Guid AuditId, string Url);

public interface IAuditQueue
{
    void Enqueue(AuditJob job);
    IAsyncEnumerable<AuditJob> ReadAllAsync(CancellationToken ct);
}
