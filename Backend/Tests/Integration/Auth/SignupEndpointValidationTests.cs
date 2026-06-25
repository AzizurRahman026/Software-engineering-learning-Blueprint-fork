using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Tests.Integration;

namespace Tests.Integration.Auth;

/// <summary>
/// End-to-end test of the validation chain for POST /api/auth/signup:
/// FluentValidation (SignupCommandValidator) -> MediatR ValidationBehavior pipeline
/// -> GlobalExceptionMiddleware -> RFC 7807 ValidationProblemDetails.
/// A bad email is rejected by the validator BEFORE the handler touches MongoDB,
/// so this test never hits the database (though it shares the same in-memory host).
/// </summary>
[Collection("integration")]
public class SignupEndpointValidationTests
{
    private readonly HttpClient _client;

    public SignupEndpointValidationTests(IntegrationTestFactory factory)
    {
        // Don't auto-follow redirects so UseHttpsRedirection can't swallow our assertion.
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Signup_WithInvalidEmail_Returns400ProblemJson_WithEmailError()
    {
        // Arrange — valid username + password, but a junk email the validator must reject.
        var badRequest = new
        {
            username = "aziz_test",
            email = "not-an-email",
            password = "supersecret123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/signup", badRequest);

        // Assert — status, media type, and the per-field error are all checked.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
        Assert.True(problem.Errors.ContainsKey("Email"),
            "Expected a validation error keyed by 'Email'.");
    }
}
