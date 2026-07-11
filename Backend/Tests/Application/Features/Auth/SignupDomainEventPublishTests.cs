using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Security;
using Application.Common.Security;
using Application.Features.Auth.Commands.Signup;
using Application.Settings;
using Domain.Entities;
using Domain.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Application.Features.Auth;

/// <summary>
/// Day 19 contract shift: SignupCommandHandler no longer publishes domain events
/// itself - dispatch moved to the persistence boundary (Repository drains and
/// publishes after a successful write, via IDomainEventDispatcher, and only for
/// AggregateRoot entities that actually raised something).
///
/// The handler's contract is now: RAISE (via User.Register) and PERSIST.
/// These tests pin that contract using a fake IUserRepository, which - like any
/// test double of the interface - does NOT dispatch. So the aggregate handed to
/// AddAsync must still be carrying its UserRegisteredEvent when the handler returns.
/// The dispatch behaviour itself belongs to Infrastructure's DomainEventDispatcher
/// and is integration-test territory.
/// </summary>
public class SignupDomainEventPublishTests
{
    private static (SignupCommandHandler handler, List<User> persisted) BuildHandler(
        bool addSucceeds = true)
    {
        var userRepo = Substitute.For<IUserRepository>();
        // Brand-new email + username: both lookups miss so registration proceeds.
        userRepo.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        userRepo.GetByUsernameAsync(Arg.Any<string>()).Returns((User?)null);

        // Capture the User the handler hands to the persistence boundary.
        var persisted = new List<User>();
        userRepo.AddAsync(Arg.Do<User>(u => persisted.Add(u))).Returns(addSucceeds);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.HashPassword(Arg.Any<string>()).Returns(("hash", "salt"));

        var validator = Substitute.For<IAuthValidator>();
        validator.IsValidEmail(Arg.Any<string>()).Returns(true);
        // ValidatePassword is void and throws on failure - leaving it as a no-op = passes.

        var tokenGenerator = Substitute.For<IJwtTokenGenerator>();
        tokenGenerator.GenerateAccessToken(Arg.Any<User>())
            .Returns(new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(60)));
        // Use the real issuer over a mocked generator so AuthResponseDto.FromUser runs for
        // real and response.UserId lines up with the persisted aggregate's id.
        var tokenIssuer = new AuthTokenIssuer(tokenGenerator, new JwtOptions());

        var handler = new SignupCommandHandler(
            userRepo, hasher, validator,
            tokenIssuer,
            new SuperAdminOptions(),
            Substitute.For<ILogger<SignupCommandHandler>>());

        return (handler, persisted);
    }

    private static SignupCommand ValidCommand() => new()
    {
        Username = "Aziz",
        Email = "aziz@example.com",
        Password = "Sup3r$ecret",
    };

    // 1. The aggregate handed to the persistence boundary carries exactly one
    //    UserRegisteredEvent with the real user's data - raised, not yet drained.
    [Fact]
    public async Task Handle_OnSuccessfulSignup_PersistsUserCarryingExactlyOneUserRegisteredEvent()
    {
        var (handler, persisted) = BuildHandler();

        var response = await handler.Handle(ValidCommand(), CancellationToken.None);

        var user = Assert.Single(persisted);
        var e = Assert.IsType<UserRegisteredEvent>(Assert.Single(user.GetDomainEvents()));
        Assert.Equal(response.UserId, e.UserId);
        Assert.Equal("aziz", e.Username);       // normalized to lower-case
        Assert.Equal("aziz@example.com", e.Email);
    }

    // 2. A failed save throws, and the event is never lost from the entity -
    //    the boundary only dispatches on success, so nothing would have fired.
    [Fact]
    public async Task Handle_WhenSaveFails_ThrowsAndEventRemainsUndispatchedOnEntity()
    {
        var (handler, persisted) = BuildHandler(addSucceeds: false);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            handler.Handle(ValidCommand(), CancellationToken.None));

        // The aggregate still holds its event: nobody drained it, because
        // dispatch is the persistence boundary's job and the write failed.
        var user = Assert.Single(persisted);
        Assert.Single(user.GetDomainEvents());
    }
}
