using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using AccessiTrack.Application.Common.Interfaces;

namespace AccessiTrack.Infrastructure.Auditing;

/// <summary>
/// In-memory unbounded channel — no Redis or external broker needed.
/// Jobs survive server restarts only if you add persistence (Hangfire / Outbox).
/// </summary>
public class AuditQueue : IAuditQueue
{
    private readonly Channel<AuditJob> _channel =
        Channel.CreateUnbounded<AuditJob>(
            new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(AuditJob job) =>
        _channel.Writer.TryWrite(job);

    public async IAsyncEnumerable<AuditJob> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(ct))
            yield return job;
    }
}
