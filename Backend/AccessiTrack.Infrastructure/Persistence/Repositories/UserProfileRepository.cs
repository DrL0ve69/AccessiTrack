using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessiTrack.Infrastructure.Persistence.Repositories;

public class UserProfileRepository(ApplicationDbContext context) : IUserProfileRepository
{
    public async Task AddAsync(UserProfile profile, CancellationToken ct)
    {
        await context.UserProfiles.AddAsync(profile, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<UserProfile?> GetByIdentityIdAsync(string identityId, CancellationToken ct)
        => await context.UserProfiles
            .FirstOrDefaultAsync(x => x.IdentityId == identityId, ct);

    public async Task<IReadOnlyList<UserProfile>> GetAllAsync(CancellationToken ct)
        => await context.UserProfiles.ToListAsync(ct);

    public async Task UpdateAsync(UserProfile profile, CancellationToken ct)
    {
        context.UserProfiles.Update(profile);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await context.UserProfiles.FindAsync(new object[] { profileId }, ct);

        // Doit vérifier si le profil existe avant de tenter de le supprimer et déclencher la suppression du appUser ou l'archiver?
        if (profile is not null)
        {
            context.UserProfiles.Remove(profile);
            await context.SaveChangesAsync(ct);
        }
    }
}
