using Application.Common.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class PostCommentRepository : Repository, IPostCommentRepository
{
    public PostCommentRepository(IDatabaseContext dbContext, IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, domainEventDispatcher)
    {
    }

    public async Task<List<PostComment>> GetByPostIdAsync(string postId)
    {
        var comments = await DbContext.GetItemsByConditionAsync<PostComment>(c => c.PostId == postId);
        return comments ?? new List<PostComment>();
    }
}
