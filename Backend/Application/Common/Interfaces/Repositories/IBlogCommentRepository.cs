using Domain.Entities;
using Domain.Repositories.Base;

namespace Application.Common.Interfaces.Repositories;

public interface IBlogCommentRepository : IRepository
{
    Task<List<BlogComment>> GetByPostIdAsync(string blogPostId);
}
