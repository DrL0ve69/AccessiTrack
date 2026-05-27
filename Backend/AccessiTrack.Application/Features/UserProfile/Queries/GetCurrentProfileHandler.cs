using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Application.Features.UserProfile.DTOs;
using AccessiTrack.Domain.Constants;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Features.UserProfile.Queries;

// 2. Le Handler
public class GetCurrentProfileHandler(
    IUserProfileRepository profileRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetCurrentProfileQuery, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(
        GetCurrentProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        // Récupération du profil en base de données
        var profile = await profileRepository.GetByIdentityIdAsync(userId, cancellationToken);

        if (profile == null)
        {
            throw new KeyNotFoundException($"Profil introuvable pour l'utilisateur {userId}.");
        }

        // Récupération du rôle depuis le service (basé sur les claims du JWT)
        var role = currentUserService.IsAdmin ? Roles.Admin : Roles.Member;

        // Mapping de l'entité vers le DTO attendu par Angular
        return new UserProfileDto(
            Id: profile.Id,
            UserId: profile.IdentityId, // Fait le pont avec Angular qui s'attend à "userId"
            Email: profile.Email,
            FullName: profile.FullName,
            PhoneNumber: profile.PhoneNumber,
            Avatar: profile.Avatar,
            Bio: profile.Bio,
            PreferredLanguage: profile.PreferredLanguage,
            HighContrastEnabled: profile.HighContrastEnabled,
            Role: role,
            EmailPrivate: profile.EmailPrivate,
            PhonePrivate: profile.PhonePrivate,
            BioPrivate: profile.BioPrivate,
            CreatedAt: profile.CreatedAt,
            UpdatedAt: profile.UpdatedAt
        );
    }
}
