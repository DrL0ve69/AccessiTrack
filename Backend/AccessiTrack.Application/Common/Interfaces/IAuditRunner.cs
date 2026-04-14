using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Common.Interfaces;

public interface IAuditRunner
{
    Task RunAsync(Guid auditId, string url, CancellationToken ct = default);
}
