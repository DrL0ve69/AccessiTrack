using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AccessiTrack.Infrastructure.Auditing;

public class AuditBackgroundService(
    IAuditQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<AuditBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Audit background service started.");

        await foreach (var job in queue.ReadAllAsync(ct))
        {
            try
            {
                // New scope per job — Playwright runner holds EF DbContext
                await using var scope = scopeFactory.CreateAsyncScope();
                var runner = scope.ServiceProvider.GetRequiredService<IAuditRunner>();
                await runner.RunAsync(job.AuditId, job.Url, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex,
                    "Unhandled error for audit job {AuditId}", job.AuditId);
            }
        }
    }
}
