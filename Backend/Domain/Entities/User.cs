using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class User : AggregateRoot
{
    // Setters are private: state changes only through the behavior methods below,
    public string Username { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;
    public string? PasswordResetTokenHash { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    // SHA-256 hash of the active refresh token (the raw token lives only on the client). One active session.
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    // Authorization role. Everyone registers as a plain User; a SuperAdmin promotes to Admin.
    public UserRole Role { get; private set; } = UserRole.User;


    private User() { }

    /// <summary>
    /// creation invariants HERE in the Domain — a normalized username and the
    /// presence of credentials — so no handler can build an invalid User.
    /// </summary>
    public static User Register(string username, Email email, string passwordHash, string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Username is required.");
        if (email is null)
            throw new ValidationException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash) || string.IsNullOrWhiteSpace(passwordSalt))
            throw new ValidationException("Password credentials are required.");

        var user = new User
        {
            Username = username.Trim().ToLowerInvariant(),
            Email = email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };
        
        /// Record "user registered" so a dispatcher can react after save (welcome email, analytics, etc.)
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Username, user.Email.Value));
        return user;
    }

    /// <summary>Sets the authorization role. Only a SuperAdmin should invoke this (enforced in the handler).</summary>
    public void AssignRole(UserRole role) => Role = role;

    /// <summary>Stores the hash + expiry of a newly issued refresh token (replaces any prior session).</summary>
    public void SetRefreshToken(string tokenHash, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ValidationException("Refresh token hash is required.");
        RefreshTokenHash = tokenHash;
        RefreshTokenExpiresAt = expiresAt;
    }

    /// <summary>Revokes the active refresh token (logout).</summary>
    public void ClearRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAt = null;
    }

    /// <summary>True when a stored refresh-token hash matches and hasn't expired.</summary>
    public bool IsRefreshTokenValid(string tokenHash, DateTime nowUtc) =>
        !string.IsNullOrEmpty(RefreshTokenHash)
        && RefreshTokenHash == tokenHash
        && RefreshTokenExpiresAt is { } exp && exp > nowUtc;

    /// <summary>Changes the username, applying the same normalization + invariant as Register.</summary>
    public void Rename(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Username is required.");
        Username = username.Trim().ToLowerInvariant();
    }

    /// <summary>Changes the email. The Email value object has already self-validated.</summary>
    public void ChangeEmail(Email email)
    {
        Email = email ?? throw new ValidationException("Email is required.");
    }

    /// <summary>Sets new password credentials (used by profile update — token state untouched).</summary>
    public void SetPassword(string passwordHash, string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || string.IsNullOrWhiteSpace(passwordSalt))
            throw new ValidationException("Password credentials are required.");
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    /// <summary>Stores a reset-token hash + expiry, beginning a password-recovery window.</summary>
    public void RequestPasswordReset(string tokenHash, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ValidationException("Reset token hash is required.");
        PasswordResetTokenHash = tokenHash;
        PasswordResetTokenExpiresAt = expiresAt;
    }

    /// <summary>
    /// Completes a reset: sets new credentials AND clears the token in one step,
    /// so the entity itself guarantees a reset link is single-use (can't be replayed).
    /// </summary>
    public void CompletePasswordReset(string passwordHash, string passwordSalt)
    {
        SetPassword(passwordHash, passwordSalt);
        PasswordResetTokenHash = null;
        PasswordResetTokenExpiresAt = null;
    }
}
