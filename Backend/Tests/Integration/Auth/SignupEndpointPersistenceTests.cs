using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Tests.Integration;

namespace Tests.Integration.Auth;

/// <summary>
/// Tests a BUSINESS INVARIANT end-to-end: "an email may register only once."
///
/// This is distinct from the other two integration tests:
///   - SignupEndpointValidationTests proves a malformed request is rejected by the
///     FluentValidation pipeline BEFORE the handler runs (never touches Mongo).
///   - SignupPersistenceTests proves a single valid signup lands in Mongo.
///
/// The duplicate-email rule lives in SignupCommandHandler, not in any validator, and it can
/// only fire if the first signup was actually WRITTEN to Mongo and then READ BACK by
/// GetByEmailAsync on the second call. So a 400 on the second request is end-to-end proof
/// of the write+read round-trip against the real (Testcontainers) database.
/// </summary>
[Collection("integration")]
public class SignupEndpointPersistenceTests
{
    private readonly HttpClient _client;

    public SignupEndpointPersistenceTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    // Handler's ValidatePassword requires a letter, a digit, AND a special character.
    private const string ValidPassword = "Supersecret123!";

    [Fact]
    public async Task Signup_WithDuplicateEmail_Returns400_ProvingFirstWritePersisted()
    {
        // Arrange — one email, two different usernames so the EMAIL guard (not the username
        // guard) is what trips on the second call. Unique email per run avoids cross-run clashes.
        var email = $"dupe_{Guid.NewGuid():N}@example.com";

        var first = new
        {
            username = $"first_{Guid.NewGuid():N}"[..16],
            email,
            password = ValidPassword
        };

        // Act 1 — first registration must succeed and persist.
        var firstResponse = await _client.PostAsJsonAsync("/api/auth/signup", first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act 2 — same email, different username.
        var second = new
        {
            username = $"second_{Guid.NewGuid():N}"[..16],
            email,
            password = ValidPassword
        };
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/signup", second);

        // Assert — the guard only triggers after reading the first user back from Mongo,
        // and the handler surfaces it as a domain ValidationException -> 400 problem+json.
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
        Assert.Equal("application/problem+json",
            secondResponse.Content.Headers.ContentType?.MediaType);
    }
}
