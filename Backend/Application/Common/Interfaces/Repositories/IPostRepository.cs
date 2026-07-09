using Domain.Entities;
using Domain.Repositories.Base;

namespace Application.Common.Interfaces.Repositories;

public interface IPostRepository : IRepository
{
    // Public feed: Published only, newest first, offset-paginated.
    Task<List<Post>> GetPagedAsync(int pageNumber, int pageSize);

    // Moderation queue: Pending posts, oldest first (first-in reviewed first).
    Task<List<Post>> GetPendingAsync();
}
