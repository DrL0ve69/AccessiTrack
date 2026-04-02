using System;
using System.Collections.Generic;
using System.Text;

using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccessiTrack.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.Projects
            .Include(p => p.Audits)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken ct)
        => await _context.Projects
            .Where(p => !p.IsArchived)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Project project, CancellationToken ct)
        => await _context.Projects.AddAsync(project, ct);

    public async Task UpdateAsync(Project project, CancellationToken ct)
        => _context.Projects.Update(project);

    public async Task<int> SaveChangesAsync(CancellationToken ct)
        => await _context.SaveChangesAsync(ct);
}
