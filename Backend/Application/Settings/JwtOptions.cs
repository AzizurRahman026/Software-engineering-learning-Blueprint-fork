namespace Application.Settings;

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // HMAC-SHA256 signing secret. Must be >= 32 bytes. Supplied via the Jwt__Key env var in production.
    public string Key { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 14;
}
