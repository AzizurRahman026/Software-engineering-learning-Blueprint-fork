using Application.Common.Interfaces.Repositories;
using Application.Features.Posts.DTOs;
using MediatR;

namespace Application.Features.Posts.Queries.GetPosts;

public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, List<PostSummaryDto>>
{
    private readonly IPostRepository _blogPostRepository;

    public GetPostsQueryHandler(IPostRepository blogPostRepository)
    {
        _blogPostRepository = blogPostRepository;
    }

    public async Task<List<PostSummaryDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var posts = await _blogPostRepository.GetPagedAsync(page, pageSize);
        return posts.Select(PostSummaryDto.FromEntity).ToList();
    }
}
