using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccessiTrack.Infrastructure.Persistence.Repositories;

public class ViolationRepository : IViolationRepository
{
    private readonly ApplicationDbContext _context;

    public ViolationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Violation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Violations
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IEnumerable<Violation>> GetByAuditIdAsync(
        Guid auditId, CancellationToken ct = default)
        => await _context.Violations
            .Where(v => v.AuditId == auditId)
            .OrderBy(v => v.Severity)
            .ToListAsync(ct);

    public async Task AddAsync(Violation violation, CancellationToken ct = default)
        => await _context.Violations.AddAsync(violation, ct);

    public async Task UpdateAsync(Violation violation, CancellationToken ct = default)
        => _context.Violations.Update(violation);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public Task AddRangeAsync(IEnumerable<Violation> violations, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
