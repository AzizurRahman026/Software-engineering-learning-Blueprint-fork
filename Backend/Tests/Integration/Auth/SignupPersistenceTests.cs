using System.Net;
using System.Net.Http.Json;
using Application.Common.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration;

namespace Tests.Integration.Auth;

/// <summary>
/// Full-flow integration test: HTTP POST -> MediatR -> SignupCommandHandler -> real MongoDB.
/// Verifies that a valid signup returns 201 Created AND the user is actually persisted.
/// </summary>
[Collection("integration")]
public class SignupPersistenceTests
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;

    public SignupPersistenceTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Signup_WithValidData_Returns201_AndPersistsUser()
    {
        // Arrange — unique email so reruns don't collide with an already-registered user.
        var email = $"aziz_int_{Guid.NewGuid():N}@example.com";
        var request = new
        {
            username = $"aziz_{Guid.NewGuid():N}"[..12],
            email,
            password = "Supersecret123!" // handler requires letter + digit + special char
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/signup", request);

        // Assert — HTTP contract
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Assert — actually landed in Mongo (read it back via the real repository).
        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var persisted = await users.GetByEmailAsync(email);

        Assert.NotNull(persisted);
        Assert.Equal(email, persisted!.Email.Value);
    }
}
