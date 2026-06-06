using Application.Features.Blog.DTOs;
using MediatR;

namespace Application.Features.Blog.Queries.GetBlogPosts;

public class GetBlogPostsQuery : IRequest<List<BlogPostSummaryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
