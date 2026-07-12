using Application.Common.Interfaces.Publisher;
using Application.Common.Interfaces.Repositories;
using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Subscribers.Commands.Subscribe;
using Application.Settings;
using Contracts.Messaging;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Application.Features.Messaging;

/// <summary>
/// Pins the "publish, don't send inline" contract after moving email off the request thread:
/// the handlers hand a typed message to IMessageBus (the MassTransit pipeline) instead of
/// calling IEmailSender directly. Rendering + delivery now live in the consumers.
/// </summary>
public class EmailPublishTests
{
    private static PasswordResetOptions Options() =>
        new() { TokenExpiryMinutes = 60, FrontendUrl = "http://localhost:4200" };

    // ── ForgotPassword: existing account → a SendPasswordResetEmail is published ──────────────
    [Fact]
    public async Task ForgotPassword_WhenUserExists_PublishesPasswordResetEmailMessage()
    {
        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByEmailAsync("aziz@example.com")
            .Returns(User.Register("aziz", Email.Create("aziz@example.com"), "hash", "salt"));

        var bus = Substitute.For<IMessageBus>();
        var handler = new ForgotPasswordCommandHandler(
            userRepo, bus, Options(), Substitute.For<ILogger<ForgotPasswordCommandHandler>>());

        await handler.Handle(new ForgotPasswordCommand { Email = "aziz@example.com" }, default);

        await bus.Received(1).PublishAsync(Arg.Is<SendPasswordResetEmail>(m =>
            m.ToEmail == "aziz@example.com" &&
            m.Username == "aziz" &&
            m.ExpiryMinutes == 60 &&
            m.ResetLink.StartsWith("http://localhost:4200/reset-password?token=")));
    }

    // ── ForgotPassword: unknown account → nothing is published (and no enumeration) ───────────
    [Fact]
    public async Task ForgotPassword_WhenUserMissing_PublishesNothing()
    {
        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        var bus = Substitute.For<IMessageBus>();
        var handler = new ForgotPasswordCommandHandler(
            userRepo, bus, Options(), Substitute.For<ILogger<ForgotPasswordCommandHandler>>());

        await handler.Handle(new ForgotPasswordCommand { Email = "ghost@example.com" }, default);

        await bus.DidNotReceive().PublishAsync(Arg.Any<SendPasswordResetEmail>());
    }

    // ── Subscribe: new address → a SendSubscriptionConfirmation is published ──────────────────
    [Fact]
    public async Task Subscribe_ForNewAddress_PublishesConfirmationEmailMessage()
    {
        var subscriberRepo = Substitute.For<ISubscriberRepository>();
        subscriberRepo.GetByEmailAsync(Arg.Any<string>()).Returns((Subscriber?)null);

        var bus = Substitute.For<IMessageBus>();
        var handler = new SubscribeCommandHandler(
            subscriberRepo, bus, Options(), Substitute.For<ILogger<SubscribeCommandHandler>>());

        await handler.Handle(new SubscribeCommand { Email = "new@example.com" }, default);

        await bus.Received(1).PublishAsync(Arg.Is<SendSubscriptionConfirmation>(m =>
            m.ToEmail == "new@example.com" &&
            m.ConfirmLink.StartsWith("http://localhost:4200/confirm-subscription?token=")));
    }
}
