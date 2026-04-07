using MediatR;
using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Application.Features.Auth.DTOs;

namespace AccessiTrack.Application.Features.Auth.Commands.Login;

public class LoginHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        return await identityService.LoginAsync(request.Email, request.Password, cancellationToken);
    }
}
