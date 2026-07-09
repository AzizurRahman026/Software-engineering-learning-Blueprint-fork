using Application.Common.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class PostLikeRepository : Repository, IPostLikeRepository
{
    public PostLikeRepository(IDatabaseContext dbContext, IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, domainEventDispatcher)
    {
    }

    public Task<PostLike?> GetByPostAndUserAsync(string postId, string userId)
    {
        return DbContext.GetItemByConditionAsync<PostLike>(
            l => l.PostId == postId && l.UserId == userId);
    }

    public async Task<List<PostLike>> GetByPostIdAsync(string postId)
    {
        var likes = await DbContext.GetItemsByConditionAsync<PostLike>(l => l.PostId == postId);
        return likes ?? new List<PostLike>();
    }
}
