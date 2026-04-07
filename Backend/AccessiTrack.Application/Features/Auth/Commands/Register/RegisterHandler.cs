using MediatR;
using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Application.Features.Auth.DTOs;

namespace AccessiTrack.Application.Features.Auth.Commands.Register;

public class RegisterHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(
        RegisterCommand request, CancellationToken cancellationToken)
    {
        return await identityService.RegisterAsync(
            request.UserName, request.Email, request.Password, cancellationToken);
    }
}
