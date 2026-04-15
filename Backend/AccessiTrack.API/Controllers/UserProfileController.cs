using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Application.Features.UserProfile.DTOs;
using AccessiTrack.Domain.Constants;
using AccessiTrack.Infrastructure.Identity;
using AccessiTrack.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccessiTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController(
    UserManager<ApplicationUser> userManager,
    IUserProfileRepository userProfileRepository,
    IFileStorageService fileStorageService
) : ControllerBase
{
    /// <summary>
    /// Get currently authenticated user's full profile
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> GetCurrentProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized("User not found in token");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound("User not found");

        var profile = await userProfileRepository.GetByIdentityIdAsync(user.Id.ToString(), cancellationToken);
        if (profile is null)
            return NotFound("User profile not found");

        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? Roles.Member;
        return Ok(MapToDto(profile, role, true));
    }

    /// <summary>
    /// Get a specific user's profile (privacy-filtered)
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(
        string userId,
        CancellationToken cancellationToken)
    {
        var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!Guid.TryParse(requestingUserId, out var requestingUserGuid))
            return Unauthorized("Invalid user ID in token");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound("User not found");

        var profile = await userProfileRepository.GetByIdentityIdAsync(user.Id.ToString(), cancellationToken);
        if (profile is null || profile.IsDeleted)
            return NotFound("User profile not found");

        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? Roles.Member;
        var isOwnProfile = requestingUserGuid == user.Id;
        var canViewPrivate = isOwnProfile || isAdmin;

        return Ok(MapToDto(profile, role, canViewPrivate));
    }

    /// <summary>
    /// Get all active users (paginated, searchable, filterable)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedUsersResponse>> GetAllUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var users = await userProfileRepository.GetAllAsync(cancellationToken);

        // Exclude soft-deleted users
        users = users.Where(u => !u.IsDeleted).ToList();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
            users = users.Where(u => u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        // Apply role filter
        if (!string.IsNullOrWhiteSpace(role))
        {
            var usersByRole = new List<Domain.Entities.UserProfile>();
            foreach (var userProfile in users)
            {
                var appUser = await userManager.FindByIdAsync(userProfile.IdentityId);
                if (appUser is not null)
                {
                    var userRoles = await userManager.GetRolesAsync(appUser);
                    if (userRoles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))
                        usersByRole.Add(userProfile);
                }
            }
            users = usersByRole;
        }

        // Apply pagination
        var totalCount = users.Count;
        var paginatedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Roles.Admin);

        var dtos = new List<UserSummaryDto>();
        foreach (var profile in paginatedUsers)
        {
            var appUser = await userManager.FindByIdAsync(profile.IdentityId);
            if (appUser is not null)
            {
                var userRole = (await userManager.GetRolesAsync(appUser)).FirstOrDefault() ?? Roles.Member;
                var isOwnProfile = requestingUserId == appUser.Id.ToString();
                var canViewPrivate = isOwnProfile || isAdmin;

                dtos.Add(new UserSummaryDto(
                    UserId: appUser.Id.ToString(),
                    Email: canViewPrivate || !profile.EmailPrivate ? profile.Email : null,
                    FullName: profile.FullName,
                    Avatar: profile.Avatar,
                    Bio: canViewPrivate || !profile.BioPrivate ? profile.Bio : null,
                    Role: userRole
                ));
            }
        }

        return Ok(new PaginatedUsersResponse(dtos, totalCount, page, pageSize));
    }

    /// <summary>
    /// Update user profile (non-role fields)
    /// </summary>
    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(
        string userId,
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!Guid.TryParse(userId, out var userGuid) || !Guid.TryParse(requestingUserId, out var requestingUserGuid))
            return BadRequest("Invalid user ID");

        // Check authorization
        if (userGuid != requestingUserGuid && !isAdmin)
            return Forbid();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound("User not found");

        var profile = await userProfileRepository.GetByIdentityIdAsync(user.Id.ToString(), cancellationToken);
        if (profile is null)
            return NotFound("User profile not found");

        // Update profile fields
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            profile.FullName = request.FullName;
            user.DisplayName = request.FullName;
        }

        if (request.PhoneNumber is not null)
            profile.PhoneNumber = request.PhoneNumber;

        if (request.Bio is not null)
            profile.Bio = request.Bio;

        if (!string.IsNullOrWhiteSpace(request.PreferredLanguage))
            profile.PreferredLanguage = request.PreferredLanguage;

        if (request.HighContrastEnabled is not null)
            profile.HighContrastEnabled = request.HighContrastEnabled.Value;

        if (request.EmailPrivate is not null)
            profile.EmailPrivate = request.EmailPrivate.Value;

        if (request.PhonePrivate is not null)
            profile.PhonePrivate = request.PhonePrivate.Value;

        if (request.BioPrivate is not null)
            profile.BioPrivate = request.BioPrivate.Value;

        profile.UpdatedAt = DateTime.UtcNow;

        // Save changes
        await userProfileRepository.UpdateAsync(profile, cancellationToken);
        await userManager.UpdateAsync(user);

        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? Roles.Member;
        return Ok(MapToDto(profile, role, true));
    }

    /// <summary>
    /// Upload user avatar
    /// </summary>
    [HttpPost("{userId}/avatar")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<object>> UploadAvatar(
        string userId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!Guid.TryParse(userId, out var userGuid) || !Guid.TryParse(requestingUserId, out var requestingUserGuid))
            return BadRequest("Invalid user ID");

        // Check authorization
        if (userGuid != requestingUserGuid && !isAdmin)
            return Forbid();

        if (file is null || file.Length == 0)
            return BadRequest("No file provided");

        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound("User not found");

            var profile = await userProfileRepository.GetByIdentityIdAsync(user.Id.ToString(), cancellationToken);
            if (profile is null)
                return NotFound("User profile not found");

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(profile.Avatar))
                await fileStorageService.DeleteAvatarAsync(profile.Avatar, cancellationToken);

            // Upload new avatar
            var avatarUrl = await fileStorageService.UploadAvatarAsync(file, profile.Id, cancellationToken);

            // Update profile
            profile.Avatar = avatarUrl;
            profile.UpdatedAt = DateTime.UtcNow;
            await userProfileRepository.UpdateAsync(profile, cancellationToken);

            return Ok(new { avatarUrl });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update user role (admin only)
    /// </summary>
    [HttpPost("{userId}/role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> UpdateUserRole(
        string userId,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Role))
            return BadRequest("Role is required");

        if (request.Role != Roles.Admin && request.Role != Roles.Member)
            return BadRequest($"Invalid role. Must be '{Roles.Admin}' or '{Roles.Member}'");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound("User not found");

        var profile = await userProfileRepository.GetByIdentityIdAsync(user.Id.ToString(), cancellationToken);
        if (profile is null)
            return NotFound("User profile not found");

        // Get current roles and remove them
        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Any())
            await userManager.RemoveFromRolesAsync(user, currentRoles);

        // Add new role
        await userManager.AddToRoleAsync(user, request.Role);

        profile.UpdatedAt = DateTime.UtcNow;
        await userProfileRepository.UpdateAsync(profile, cancellationToken);

        return Ok(MapToDto(profile, request.Role, true));
    }

    /// <summary>
    /// Soft delete user profile
    /// </summary>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProfile(
        string userId,
        CancellationToken cancellationToken)
    {
        var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Roles.Admin);

        // Check authorization
        if (userId != requestingUserId && !isAdmin)
            return Forbid();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound("User not found");

        var profile = await userProfileRepository.GetByIdentityIdAsync(user.Id.ToString(), cancellationToken);
        if (profile is null)
            return NotFound("User profile not found");

        // Soft delete
        profile.IsDeleted = true;
        profile.DeletedAt = DateTime.UtcNow;

        // Delete avatar file if exists
        if (!string.IsNullOrEmpty(profile.Avatar))
            await fileStorageService.DeleteAvatarAsync(profile.Avatar, cancellationToken);

        await userProfileRepository.UpdateAsync(profile, cancellationToken);

        return NoContent();
    }

    private static UserProfileDto MapToDto(
        Domain.Entities.UserProfile profile,
        string role,
        bool showPrivate = false)
    {
        return new UserProfileDto(
            Id: profile.Id,
            UserId: profile.IdentityId,
            Email: showPrivate || !profile.EmailPrivate ? profile.Email : null!,
            FullName: profile.FullName,
            PhoneNumber: showPrivate || !profile.PhonePrivate ? profile.PhoneNumber : null,
            Avatar: profile.Avatar,
            Bio: showPrivate || !profile.BioPrivate ? profile.Bio : null,
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
