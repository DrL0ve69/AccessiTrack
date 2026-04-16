using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace AccessiTrack.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    public string UserId =>
        httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? string.Empty;

    public string Email =>
        httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.Email)
        ?? string.Empty;

    public bool IsAdmin =>
        httpContextAccessor.HttpContext?
            .User.IsInRole(Roles.Admin)
        ?? false;
}
