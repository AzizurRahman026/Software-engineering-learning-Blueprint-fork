using Domain.Entities;
using Domain.Repositories.Base;

namespace Application.Common.Interfaces.Repositories;

public interface IBlogPostRepository : IRepository
{
    // Public feed: newest first, offset-paginated.
    Task<List<BlogPost>> GetPagedAsync(int pageNumber, int pageSize);
}
