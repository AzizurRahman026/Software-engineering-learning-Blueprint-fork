using Application.Common.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class PostRepository : Repository, IPostRepository
{
    public PostRepository(IDatabaseContext dbContext, IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, domainEventDispatcher)
    {
    }

    public Task<List<Post>> GetPagedAsync(int pageNumber, int pageSize)
    {
        // Public feed shows only published posts.
        return DbContext.GetPagedResponseAsync<Post>(
            criteria: p => p.Status == PostStatus.Published,
            pageNumber: pageNumber,
            pageSize: pageSize,
            orderBy: p => p.PublishedAt,
            ascending: false);
    }

    public async Task<List<Post>> GetPendingAsync()
    {
        var pending = await DbContext.GetItemsByConditionAsync<Post>(p => p.Status == PostStatus.Pending);
        return (pending ?? new List<Post>())
            .OrderBy(p => p.CreatedAt)
            .ToList();
    }
}
