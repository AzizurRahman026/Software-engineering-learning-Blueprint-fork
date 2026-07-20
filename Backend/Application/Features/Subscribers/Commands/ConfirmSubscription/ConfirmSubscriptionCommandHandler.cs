using Application.Common.Interfaces.Repositories;
using Application.Common.Security;
using Application.Features.Subscribers.DTOs;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Subscribers.Commands.ConfirmSubscription;

public class ConfirmSubscriptionCommandHandler : IRequestHandler<ConfirmSubscriptionCommand, SubscriptionResponseDto>
{
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly ILogger<ConfirmSubscriptionCommandHandler> _logger;

    public ConfirmSubscriptionCommandHandler(ISubscriberRepository subscriberRepository, ILogger<ConfirmSubscriptionCommandHandler> logger)
    {
        _subscriberRepository = subscriberRepository;
        _logger = logger;
    }

    public async Task<SubscriptionResponseDto> Handle(ConfirmSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ValidationException("Confirmation token is required.");

        var tokenHash = ResetTokenUtil.Hash(request.Token);
        var subscriber = await _subscriberRepository.GetByTokenHashAsync(tokenHash)
            ?? throw new ValidationException("Invalid or expired confirmation link.");

        if (subscriber.Status != SubscriptionStatus.Confirmed)
        {
            subscriber.Status = SubscriptionStatus.Confirmed;
            subscriber.ConfirmedAt = DateTime.UtcNow;
            // Single-use: clear the confirm token so the link can't be replayed. Unsubscribe uses its own token.
            subscriber.ConfirmationTokenHash = null;
            await _subscriberRepository.UpdateAsync(subscriber);
            _logger.LogInformation($"Subscribed Successfully for email address: {subscriber.Email.Value}");
        }

        return new SubscriptionResponseDto("Your subscription is confirmed — welcome aboard!");
    }
}
