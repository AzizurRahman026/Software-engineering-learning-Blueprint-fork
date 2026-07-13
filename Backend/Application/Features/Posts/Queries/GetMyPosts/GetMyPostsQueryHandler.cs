using Application.Common.Interfaces.Repositories;
using Application.Features.Posts.DTOs;
using MediatR;

namespace Application.Features.Posts.Queries.GetMyPosts;

public class GetMyPostsQueryHandler : IRequestHandler<GetMyPostsQuery, List<PostSummaryDto>>
{
    private readonly IPostRepository _postRepository;

    public GetMyPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<PostSummaryDto>> Handle(GetMyPostsQuery request, CancellationToken cancellationToken)
    {
        // No role gate: any authenticated user, scoped to their own AuthorId. Returns every
        // status (Pending/Published/Rejected) so authors can see and manage their own posts.
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var posts = await _postRepository.GetByAuthorAsync(request.AuthorId, page, pageSize);
        return posts.Select(PostSummaryDto.FromEntity).ToList();
    }
}
