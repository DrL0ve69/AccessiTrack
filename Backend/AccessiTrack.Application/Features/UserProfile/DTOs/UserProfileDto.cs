namespace AccessiTrack.Application.Features.UserProfile.DTOs;

/// <summary>
/// Full user profile DTO with all details (privacy-filtered as needed)
/// </summary>
public sealed record UserProfileDto(
    Guid Id,
    string UserId,
    string Email,
    string FullName,
    string? PhoneNumber,
    string? Avatar,
    string? Bio,
    string PreferredLanguage,
    bool HighContrastEnabled,
    string Role,
    bool EmailPrivate,
    bool PhonePrivate,
    bool BioPrivate,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Public user summary for members list (respects privacy flags)
/// </summary>
public sealed record UserSummaryDto(
    string UserId,
    string? Email,
    string FullName,
    string? Avatar,
    string? Bio,
    string Role
);

/// <summary>
/// Request to update user profile (no role field)
/// </summary>
public sealed record UpdateUserProfileRequest(
    string? FullName = null,
    string? PhoneNumber = null,
    string? Bio = null,
    string? PreferredLanguage = null,
    bool? HighContrastEnabled = null,
    bool? EmailPrivate = null,
    bool? PhonePrivate = null,
    bool? BioPrivate = null
);

/// <summary>
/// Request to update user role (admin only)
/// </summary>
public sealed record UpdateUserRoleRequest(
    string Role
);

/// <summary>
/// Paginated response for user lists
/// </summary>
public sealed record PaginatedUsersResponse(
    IEnumerable<UserSummaryDto> Users,
    int TotalCount,
    int Page,
    int PageSize
);
