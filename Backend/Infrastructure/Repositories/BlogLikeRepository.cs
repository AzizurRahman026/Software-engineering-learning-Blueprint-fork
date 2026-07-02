using Application.Common.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class BlogLikeRepository : Repository, IBlogLikeRepository
{
    public BlogLikeRepository(IDatabaseContext dbContext, IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, domainEventDispatcher)
    {
    }

    public Task<BlogLike?> GetByPostAndUserAsync(string blogPostId, string userId)
    {
        return DbContext.GetItemByConditionAsync<BlogLike>(
            l => l.BlogPostId == blogPostId && l.UserId == userId);
    }

    public async Task<List<BlogLike>> GetByPostIdAsync(string blogPostId)
    {
        var likes = await DbContext.GetItemsByConditionAsync<BlogLike>(l => l.BlogPostId == blogPostId);
        return likes ?? new List<BlogLike>();
    }
}
