using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Tests.Integration;

namespace Tests.Integration.Auth;

/// <summary>
/// End-to-end JWT flow: signup mints tokens, a protected endpoint enforces the bearer token,
/// and refresh rotates (invalidating the used refresh token).
/// </summary>
[Collection("integration")]
public class JwtAuthenticationTests
{
    private readonly IntegrationTestFactory _factory;
    private const string ValidPassword = "Supersecret123!";

    public JwtAuthenticationTests(IntegrationTestFactory factory) => _factory = factory;

    private HttpClient NewClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private async Task<AuthDto> SignupAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/signup", new
        {
            username = $"jwt_{Guid.NewGuid():N}"[..16],
            email = $"jwt_{Guid.NewGuid():N}@example.com",
            password = ValidPassword
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AuthDto>())!;
    }

    [Fact]
    public async Task Signup_ReturnsTokens_AndProtectedEndpointRequiresBearer()
    {
        var client = NewClient();
        var auth = await SignupAsync(client);

        Assert.False(string.IsNullOrWhiteSpace(auth.token));
        Assert.False(string.IsNullOrWhiteSpace(auth.refreshToken));

        var body = new { title = "A JWT test post", content = "Body content long enough to pass validation." };

        // No token → 401.
        var unauth = await NewClient().PostAsJsonAsync("/api/posts", body);
        Assert.Equal(HttpStatusCode.Unauthorized, unauth.StatusCode);

        // With bearer token → 201.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);
        var created = await client.PostAsJsonAsync("/api/posts", body);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
    }

    [Fact]
    public async Task Refresh_RotatesToken_AndOldRefreshTokenIsRejected()
    {
        var client = NewClient();
        var auth = await SignupAsync(client);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = auth.refreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshed = (await refreshResponse.Content.ReadFromJsonAsync<AuthDto>())!;
        Assert.False(string.IsNullOrWhiteSpace(refreshed.token));

        // The original refresh token was rotated away, so reusing it fails.
        var reuse = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = auth.refreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    private record AuthDto(string userId, string username, string email, string role, string token, string refreshToken);
}
