using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Common.Security;

/// <summary>
/// Header-based authorization helper (auth is X-User-Id, no JWT claims). Handlers that
/// require elevated privileges resolve the acting user and check their role here.
/// </summary>
public static class RoleGuard
{
    /// <summary>Throws <see cref="AuthenticationException"/> unless the acting user is Admin or SuperAdmin.</summary>
    public static async Task EnsureAdminAsync(IUserRepository userRepository, string actingUserId)
    {
        if (string.IsNullOrWhiteSpace(actingUserId))
            throw new AuthenticationException("Authentication required.");

        var user = await userRepository.GetByIdAsync<User>(actingUserId);
        if (user is null || user.Role is not (UserRole.Admin or UserRole.SuperAdmin))
            throw new AuthenticationException("Admin privileges required.");
    }

    /// <summary>Throws <see cref="AuthenticationException"/> unless the acting user is a SuperAdmin.</summary>
    public static async Task EnsureSuperAdminAsync(IUserRepository userRepository, string actingUserId)
    {
        if (string.IsNullOrWhiteSpace(actingUserId))
            throw new AuthenticationException("Authentication required.");

        var user = await userRepository.GetByIdAsync<User>(actingUserId);
        if (user is null || user.Role != UserRole.SuperAdmin)
            throw new AuthenticationException("Super admin privileges required.");
    }
}
