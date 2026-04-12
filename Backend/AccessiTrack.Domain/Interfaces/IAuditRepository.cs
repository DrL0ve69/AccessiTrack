using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Entities;

namespace AccessiTrack.Domain.Interfaces;

public interface IAuditRepository
{
    Task<Audit?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Audit>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<Audit?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Audit audit, CancellationToken ct = default);
    Task UpdateAsync(Audit audit, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
