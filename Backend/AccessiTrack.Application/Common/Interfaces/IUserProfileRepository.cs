using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Entities;

namespace AccessiTrack.Application.Common.Interfaces;

public interface IUserProfileRepository
{
    Task AddAsync(UserProfile profile, CancellationToken ct);
    Task<UserProfile?> GetByIdentityIdAsync(string identityId, CancellationToken ct);
    Task UpdateAsync(UserProfile profile, CancellationToken ct);
    Task DeleteAsync(Guid profileId, CancellationToken ct);
}
