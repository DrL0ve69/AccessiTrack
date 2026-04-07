namespace AccessiTrack.Application.Features.Auth.DTOs;

public record AuthResponseDto(
    string Token,
    string UserId,
    string Email,
    string Role,
    DateTime ExpiresAt
);
