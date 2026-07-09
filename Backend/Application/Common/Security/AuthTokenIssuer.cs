using Application.Common.Interfaces.Security;
using Application.Features.Auth.DTOs;
using Application.Settings;
using Domain.Entities;

namespace Application.Common.Security;

/// <summary>
/// Mints an access token + a rotated refresh token for a user and stamps the refresh-token
/// hash onto the user. The caller is responsible for persisting the user afterwards
/// (AddAsync on signup, UpdateAsync on login/refresh).
/// </summary>
public static class AuthTokenIssuer
{
    public static AuthResponseDto Issue(IJwtTokenGenerator generator, JwtOptions options, User user)
    {
        var access = generator.GenerateAccessToken(user);

        var rawRefresh = ResetTokenUtil.GenerateRawToken();
        user.SetRefreshToken(ResetTokenUtil.Hash(rawRefresh), DateTime.UtcNow.AddDays(options.RefreshTokenDays));

        return AuthResponseDto.FromUser(user).WithTokens(access.Token, rawRefresh, access.ExpiresAtUtc);
    }
}
