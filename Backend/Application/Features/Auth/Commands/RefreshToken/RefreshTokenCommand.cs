using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<AuthResponseDto>
{
    // Raw refresh token issued at login/signup (or the previous refresh).
    public string RefreshToken { get; set; } = string.Empty;
}
