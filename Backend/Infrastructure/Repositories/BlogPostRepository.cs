using Application.Common.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class BlogPostRepository : Repository, IBlogPostRepository
{
    public BlogPostRepository(IDatabaseContext dbContext, IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, domainEventDispatcher)
    {
    }

    public Task<List<BlogPost>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return DbContext.GetPagedResponseAsync<BlogPost>(
            criteria: null,
            pageNumber: pageNumber,
            pageSize: pageSize,
            orderBy: p => p.CreatedAt,
            ascending: false);
    }
}
