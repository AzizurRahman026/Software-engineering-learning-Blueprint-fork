using Domain.Entities;
using Domain.Repositories.Base;

namespace Application.Common.Interfaces.Repositories;

public interface IPostCommentRepository : IRepository
{
    Task<List<PostComment>> GetByPostIdAsync(string postId);
}
