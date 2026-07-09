using Application.Features.Auth.Commands.RefreshToken;

namespace Application.Features.Auth.DTOs;

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;

    public RefreshTokenCommand ToRefreshTokenCommand() => new() { RefreshToken = RefreshToken };
}
