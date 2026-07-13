using Domain.Entities;
using Domain.Repositories.Base;

namespace Application.Common.Interfaces.Repositories;

public interface IPostRepository : IRepository
{
    // Public feed: Published only, newest first, offset-paginated.
    Task<List<Post>> GetPagedAsync(int pageNumber, int pageSize);

    // Moderation queue: Pending posts, oldest first (first-in reviewed first).
    Task<List<Post>> GetPendingAsync();

    // Author dashboard: all of one author's posts (any status), newest first.
    Task<List<Post>> GetByAuthorAsync(string authorId, int pageNumber, int pageSize);
}
