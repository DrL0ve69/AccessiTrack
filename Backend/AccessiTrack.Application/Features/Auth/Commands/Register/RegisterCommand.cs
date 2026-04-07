using MediatR;
using AccessiTrack.Application.Features.Auth.DTOs;

namespace AccessiTrack.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string UserName,
    string Email,
    string Password
) : IRequest<AuthResponseDto>;
