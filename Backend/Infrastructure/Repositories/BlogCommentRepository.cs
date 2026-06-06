using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class BlogCommentRepository : Repository, IBlogCommentRepository
{
    public BlogCommentRepository(IDatabaseContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<BlogComment>> GetByPostIdAsync(string blogPostId)
    {
        var comments = await DbContext.GetItemsByConditionAsync<BlogComment>(c => c.BlogPostId == blogPostId);
        return comments ?? new List<BlogComment>();
    }
}
