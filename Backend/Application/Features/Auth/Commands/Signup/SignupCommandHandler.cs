using Application.Common.Events;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Security;
using Application.Features.Auth.DTOs;
using Domain.Common;
using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.Signup;

public class SignupCommandHandler : IRequestHandler<SignupCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthValidator _authValidator;
    private readonly IPublisher _publisher;
    private readonly ILogger<SignupCommandHandler> _logger;

    public SignupCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuthValidator authValidator,
        IPublisher publisher,
        ILogger<SignupCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _authValidator = authValidator;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        var username = (request.Username ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim();
        var password = request.Password ?? string.Empty;

        // Tag all logs below with Username for traceability. Email is PII — never log it.
        using var _ = _logger.BeginScope(new Dictionary<string, object> { ["Username"] = username });

        _logger.LogInformation("Processing signup request");

        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Username is required.");

        if (!_authValidator.IsValidEmail(email))
            throw new ValidationException("Invalid email address.");

        _authValidator.ValidatePassword(password);

        var normalizedEmail = email.ToLowerInvariant();
        var normalizedUsername = username.ToLowerInvariant();

        if (await _userRepository.GetByEmailAsync(normalizedEmail) is not null)
            throw new ValidationException("A user with this email is already registered.");

        if (await _userRepository.GetByUsernameAsync(normalizedUsername) is not null)
            throw new ValidationException("A user with this username is already registered.");

        var (hash, salt) = _passwordHasher.HashPassword(password);

        // Creation invariants now live in the Domain entity, not here.
        var user = User.Register(normalizedUsername, Email.Create(normalizedEmail), hash, salt);

        var added = await _userRepository.AddAsync(user);
        if (!added)
            throw new UnknownException("Failed to register user.");

        _logger.LogInformation("User registered successfully with {UserId}", user.Id);

        // Dispatch domain events AFTER the Mongo write commits, so side-effects
        // (welcome email, analytics) fire only on a successful save.
        await DispatchDomainEventsAsync(user, cancellationToken);

        return AuthResponseDto.FromUser(user);
    }

    private async Task DispatchDomainEventsAsync(BaseEntity entity, CancellationToken cancellationToken)
    {
        var events = entity.GetDomainEvents().ToList();
        entity.ClearDomainEvents(); // clear first so a re-entrant publish can't double-fire

        foreach (var domainEvent in events)
            await _publisher.Publish(new DomainEventNotification(domainEvent), cancellationToken);
    }
}
