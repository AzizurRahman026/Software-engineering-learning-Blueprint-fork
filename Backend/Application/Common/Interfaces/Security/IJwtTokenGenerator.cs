using Domain.Entities;

namespace Application.Common.Interfaces.Security;

public interface IJwtTokenGenerator
{
    // Issues a signed access token carrying the user's id, username, email and role claims.
    AccessTokenResult GenerateAccessToken(User user);
}

public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
