
using Application.Common.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class UserRepository : Repository, IUserRepository
{
    public UserRepository(IDatabaseContext dbContext, IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, domainEventDispatcher)
    {
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return Email.TryCreate(email, out var parsed)
            ? DbContext.GetItemByConditionAsync<User>(u => u.Email == parsed)
            : Task.FromResult<User?>(null);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return DbContext.GetItemByConditionAsync<User>(u => u.Username == normalized);
    }

    public Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername)
    {
        var normalized = emailOrUsername.Trim().ToLowerInvariant();
        return Email.TryCreate(normalized, out var email)
            ? DbContext.GetItemByConditionAsync<User>(u => u.Email == email || u.Username == normalized)
            : DbContext.GetItemByConditionAsync<User>(u => u.Username == normalized);
    }

    public Task<User?> GetByPasswordResetTokenHashAsync(string tokenHash)
    {
        return DbContext.GetItemByConditionAsync<User>(u => u.PasswordResetTokenHash == tokenHash);
    }
}
