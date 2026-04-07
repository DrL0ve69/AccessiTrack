using AccessiTrack.Application.Features.Auth.DTOs;

namespace AccessiTrack.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken ct);
    Task<AuthResponseDto> RegisterAsync(string userName, string email, string password, CancellationToken ct);
    Task DeleteUserAsync(string userId, CancellationToken ct = default);
    Task UpdateUserRoleAsync(string userId, string role, CancellationToken ct = default);
}
