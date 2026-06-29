using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Tests.Integration;

namespace Tests.Integration.Auth;

/// <summary>
/// End-to-end proof of the Day 9 correlation-id chain for POST /api/auth/signup:
/// CorrelationIdMiddleware (reuses an inbound X-Correlation-Id OR mints a fresh one, opens the log
/// scope, echoes the header) -> GlobalExceptionMiddleware (copies the id into
/// ProblemDetails.Extensions["correlationId"]). We send a junk email so the request fails fast in
/// the FluentValidation pipeline (never touches MongoDB) yet still travels through BOTH middlewares
/// — the validation failure is exactly what triggers the ProblemDetails enrichment.
///
/// The [Theory] exercises BOTH branches of the middleware:
///   - supplied  -> the caller's id is reused verbatim on header + body.
///   - minted    -> with no inbound header, a fresh id is generated and STILL appears, identically,
///                  on header + body.
/// In every case the header id and body id must match each other, which proves the middleware
/// ordering (CorrelationId is outermost, so its context.Items entry exists before the exception
/// middleware reads it) and the enrichment work together.
/// </summary>
[Collection("integration")]
public class CorrelationIdPropagationTests
{
    private const string HeaderName = "X-Correlation-Id";

    private readonly HttpClient _client;

    public CorrelationIdPropagationTests(IntegrationTestFactory factory)
    {
        // Don't auto-follow redirects so UseHttpsRedirection can't swallow our assertion.
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData(true)]   // caller supplies an id -> it must be reused verbatim.
    [InlineData(false)]  // no inbound id -> middleware mints one and still threads it through.
    public async Task Signup_PropagatesCorrelationId_ToHeaderAndProblemBody(bool supplyId)
    {
        // Arrange — junk email so the validator rejects the request and the ProblemDetails path runs.
        var suppliedId = supplyId ? $"itest-{Guid.NewGuid():N}" : null;

        // Act
        var (response, headerId, bodyId) = await PostInvalidSignupAsync(suppliedId);

        // Assert — still a 400 problem+json (validation chain unchanged) ...
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        // ... the id is present on BOTH the header and the body, and they agree with each other.
        Assert.False(string.IsNullOrWhiteSpace(headerId),
            $"Expected the response to carry a non-empty '{HeaderName}' header.");
        Assert.Equal(headerId, bodyId);

        // When the caller supplied an id it must be reused verbatim, not regenerated.
        if (suppliedId is not null)
            Assert.Equal(suppliedId, headerId);
    }

    /// <summary>
    /// POSTs an invalid signup (optionally with an inbound X-Correlation-Id) and pulls the
    /// correlation id back out of both the response header and the problem body's
    /// <c>correlationId</c> extension. Returns the raw response so callers can assert status/media.
    /// </summary>
    private async Task<(HttpResponseMessage Response, string? HeaderId, string? BodyId)> PostInvalidSignupAsync(
        string? suppliedId)
    {
        var badRequest = new
        {
            username = "aziz_test",
            email = "not-an-email",
            password = "supersecret123"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(badRequest)
        };
        if (suppliedId is not null)
            request.Headers.TryAddWithoutValidation(HeaderName, suppliedId);

        var response = await _client.SendAsync(request);

        var headerId = response.Headers.TryGetValues(HeaderName, out var echoed)
            ? echoed.FirstOrDefault()
            : null;

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        var bodyId = problem is not null
                     && problem.Extensions.TryGetValue("correlationId", out var raw)
            // Extensions deserialize as JsonElement; fall back to ToString for other shapes.
            ? (raw is JsonElement el ? el.GetString() : raw?.ToString())
            : null;

        return (response, headerId, bodyId);
    }
}
