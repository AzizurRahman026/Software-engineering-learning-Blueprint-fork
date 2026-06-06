using Domain.Entities;
using Domain.Repositories.Base;

namespace Application.Common.Interfaces.Repositories;

public interface IBlogLikeRepository : IRepository
{
    Task<BlogLike?> GetByPostAndUserAsync(string blogPostId, string userId);
    Task<List<BlogLike>> GetByPostIdAsync(string blogPostId);
}
