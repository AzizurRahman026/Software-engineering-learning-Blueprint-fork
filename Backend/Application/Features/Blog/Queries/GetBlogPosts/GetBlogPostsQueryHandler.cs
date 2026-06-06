using Application.Common.Interfaces.Repositories;
using Application.Features.Blog.DTOs;
using MediatR;

namespace Application.Features.Blog.Queries.GetBlogPosts;

public class GetBlogPostsQueryHandler : IRequestHandler<GetBlogPostsQuery, List<BlogPostSummaryDto>>
{
    private readonly IBlogPostRepository _blogPostRepository;

    public GetBlogPostsQueryHandler(IBlogPostRepository blogPostRepository)
    {
        _blogPostRepository = blogPostRepository;
    }

    public async Task<List<BlogPostSummaryDto>> Handle(GetBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var posts = await _blogPostRepository.GetPagedAsync(page, pageSize);
        return posts.Select(BlogPostSummaryDto.FromEntity).ToList();
    }
}
