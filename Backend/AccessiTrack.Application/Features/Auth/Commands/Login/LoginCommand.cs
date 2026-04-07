using MediatR;
using AccessiTrack.Application.Features.Auth.DTOs;

namespace AccessiTrack.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
