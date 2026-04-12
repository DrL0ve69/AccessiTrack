using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccessiTrack.Infrastructure.Persistence.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly ApplicationDbContext _context;

    public AuditRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Audit?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Audits
            .Include(a => a.Violations)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IEnumerable<Audit>> GetByProjectIdAsync(
        Guid projectId, CancellationToken ct = default)
        => await _context.Audits
            .Include(a => a.Violations)
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Audit audit, CancellationToken ct = default)
        => await _context.Audits.AddAsync(audit, ct);

    public async Task UpdateAsync(Audit audit, CancellationToken ct = default)
        => _context.Audits.Update(audit);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public Task<Audit?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
