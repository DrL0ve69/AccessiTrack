using System.Linq;
using AccessiTrack.Application.Common.Exceptions;
using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Application.Features.Auth.DTOs;
using AccessiTrack.Infrastructure.Identity;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;

namespace AccessiTrack.Infrastructure.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService) : IIdentityService
{
    public async Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var isValid = await userManager.CheckPasswordAsync(user, password);
        if (!isValid)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await userManager.GetRolesAsync(user);
        var token = await tokenService.GenerateTokenAsync(
            user.Id.ToString(), user.Email!, roles);

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
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            DisplayName = userName
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var failures = result.Errors.Select(e =>
            {
                string propertyName = e.Code switch
                {
                    var c when c.Contains("Password") => "Password",
                    var c when c.Contains("Email") => "Email",
                    var c when c.Contains("UserName") || c.Contains("User") => "UserName",
                    _ => "General"
                };

                return new ValidationFailure(propertyName, e.Description);
            });

            throw new ValidationException(failures);
        }

        // Assign Member role by default
        await userManager.AddToRoleAsync(user, "Member");

        var roles = await userManager.GetRolesAsync(user);
        var token = await tokenService.GenerateTokenAsync(
            user.Id.ToString(), user.Email!, roles);

        return new AuthResponseDto(
            token,
            user.Id.ToString(),
            user.Email!,
            roles.FirstOrDefault() ?? "Member",
            DateTime.UtcNow.AddHours(8)
        );
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
