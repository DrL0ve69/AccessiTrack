using AccessiTrack.Application.Common.Exceptions;
using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Application.Features.Auth.DTOs;
using AccessiTrack.Domain.Entities;
using AccessiTrack.Infrastructure.Identity;
using AccessiTrack.Infrastructure.Persistence.Repositories;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace AccessiTrack.Infrastructure.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IUserProfileRepository userProfileRepository) : IIdentityService
{
    public async Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email)
            ?? throw new UnauthorizedAccessException("Identifiants invalides.");

        if (!await userManager.CheckPasswordAsync(user, password))
            throw new UnauthorizedAccessException("Identifiants invalides.");

        var roles = await userManager.GetRolesAsync(user);
        var token = await tokenService.GenerateTokenAsync(user.Id.ToString(), user.Email!, roles);

        return new AuthResponseDto(
            token,
            user.Id.ToString(),
            user.Email!,
            roles.FirstOrDefault() ?? "Member",
            DateTime.UtcNow.AddHours(8)
        );
    }

    public async Task<AuthResponseDto> RegisterAsync(string userName, string email, string password, CancellationToken ct)
    {
        // 1. Création du compte de sécurité
        var user = new ApplicationUser
        {
            UserName = email, // Souvent préférable d'utiliser l'email comme username
            Email = email,
            DisplayName = userName
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure("General", e.Description)));

        await userManager.AddToRoleAsync(user, "Member");

        // 2. CRÉATION DU PROFIL MÉTIER (Lien Identité -> Domaine)
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            IdentityId = user.Id.ToString(),
            FullName = userName,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        await userProfileRepository.AddAsync(profile, ct);

        // 3. Réponse
        var roles = await userManager.GetRolesAsync(user);
        var token = await tokenService.GenerateTokenAsync(user.Id.ToString(), user.Email!, roles);

        return new AuthResponseDto(token, user.Id.ToString(), user.Email!, "Member", DateTime.UtcNow.AddHours(8));
    }

    public async Task DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
            await userManager.DeleteAsync(user);
    }

    public async Task UpdateUserRoleAsync(string userId, string role, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId)
                 ?? throw new InvalidOperationException($"User with Id '{userId}' not found.");

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Any())
            await userManager.RemoveFromRolesAsync(user, currentRoles);

        await userManager.AddToRoleAsync(user, role);
    }
}
