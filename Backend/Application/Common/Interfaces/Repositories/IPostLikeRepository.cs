using Domain.Entities;
using Domain.Repositories.Base;

namespace Application.Common.Interfaces.Repositories;

public interface IPostLikeRepository : IRepository
{
    Task<PostLike?> GetByPostAndUserAsync(string postId, string userId);
    Task<List<PostLike>> GetByPostIdAsync(string postId);
}
