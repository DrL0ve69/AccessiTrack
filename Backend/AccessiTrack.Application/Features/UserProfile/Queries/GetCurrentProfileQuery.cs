using AccessiTrack.Application.Features.UserProfile.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Features.UserProfile.Queries;

// 1. La Requête (ne prend aucun paramètre car on déduit l'utilisateur du Token)
public record GetCurrentProfileQuery : IRequest<UserProfileDto>;
