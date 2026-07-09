
using Domain.Entities;
using Domain.Repositories.Base;

namespace Application.Common.Interfaces.Repositories;

public interface IUserRepository : IRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername);
    Task<User?> GetByPasswordResetTokenHashAsync(string tokenHash);
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash);
}
