using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Entities;

namespace AccessiTrack.Domain.Interfaces;

public interface IViolationRepository
{
    Task<Violation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Violation>> GetByAuditIdAsync(Guid auditId, CancellationToken ct = default);
    Task AddAsync(Violation violation, CancellationToken ct = default);
    Task UpdateAsync(Violation violation, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
