using AccessiTrack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Project>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
