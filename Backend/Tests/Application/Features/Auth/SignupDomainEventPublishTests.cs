using Application.Common.Events;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Security;
using Application.Features.Auth.Commands.Signup;
using Domain.Entities;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Application.Features.Auth;

/// <summary>
/// Verifies that SignupCommandHandler PUBLISHES domain events correctly.
/// What listeners do with them (DomainEventHandler's side-effects) is not
/// covered here — that would need the handler wired in explicitly.
/// </summary>
public class SignupDomainEventPublishTests
{
    // Records whatever is published, regardless of which IPublisher overload fires.
    private sealed class SpyPublisher : IPublisher
    {
        public List<object> Published { get; } = new();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            Published.Add(notification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Publish((object)notification, cancellationToken);
    }

    private static (SignupCommandHandler handler, SpyPublisher publisher, List<User> persisted) BuildHandler(
        bool addSucceeds = true)
    {
        var userRepo = Substitute.For<IUserRepository>();
        // Brand-new email + username: both lookups miss so registration proceeds.
        userRepo.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        userRepo.GetByUsernameAsync(Arg.Any<string>()).Returns((User?)null);

        // Remember the User the handler saves so test 2
        // can check its event buffer was drained.
        var persisted = new List<User>();
        userRepo.AddAsync(Arg.Do<User>(u => persisted.Add(u))).Returns(addSucceeds);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.HashPassword(Arg.Any<string>()).Returns(("hash", "salt"));

        var validator = Substitute.For<IAuthValidator>();
        validator.IsValidEmail(Arg.Any<string>()).Returns(true);
        // ValidatePassword is void and throws on failure — leaving it as a no-op = passes.

        var publisher = new SpyPublisher();

        var handler = new SignupCommandHandler(
            userRepo, hasher, validator, publisher,
            Substitute.For<ILogger<SignupCommandHandler>>());

        return (handler, publisher, persisted);
    }

    private static SignupCommand ValidCommand() => new()
    {
        Username = "Aziz",
        Email = "aziz@example.com",
        Password = "Sup3r$ecret",
    };

    // ── 1. The handler publishes exactly one UserRegistered notification, carrying the new id ──
    [Fact]
    public async Task Handle_OnSuccessfulSignup_PublishesExactlyOneUserRegisteredNotification()
    {
        var (handler, publisher, _) = BuildHandler();

        var response = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Exactly one event, wrapped in the MediatR adapter, carrying the real user's data.
        var notification = Assert.IsType<DomainEventNotification>(Assert.Single(publisher.Published));
        var e = Assert.IsType<UserRegisteredEvent>(notification.DomainEvent);
        Assert.Equal(response.UserId, e.UserId);
        Assert.Equal("aziz", e.Username);       // normalized to lower-case
        Assert.Equal("aziz@example.com", e.Email);
    }

    // ── 2. Clear-then-publish: the entity's event buffer is empty after dispatch ──────────────
    [Fact]
    public async Task Handle_AfterDispatch_DrainsTheEntitysDomainEvents()
    {
        var (handler, _, persisted) = BuildHandler();

        await handler.Handle(ValidCommand(), CancellationToken.None);

        // The User the repository received had its events raised (1), then cleared (0) before
        // the handler returned — so re-publishing the same aggregate is a no-op.
        var user = Assert.Single(persisted);
        Assert.Empty(user.GetDomainEvents());
    }

    // ── 3. A failed save publishes nothing — side-effects fire only on a real write ───────────
    [Fact]
    public async Task Handle_WhenSaveFails_PublishesNoNotification()
    {
        var (handler, publisher, _) = BuildHandler(addSucceeds: false);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            handler.Handle(ValidCommand(), CancellationToken.None));

        Assert.Empty(publisher.Published);
    }
}
