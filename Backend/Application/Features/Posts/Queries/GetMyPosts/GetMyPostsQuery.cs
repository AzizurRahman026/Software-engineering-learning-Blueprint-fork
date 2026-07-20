using Application.Features.Posts.DTOs;
using MediatR;

namespace Application.Features.Posts.Queries.GetMyPosts;

public class GetMyPostsQuery : IRequest<List<PostSummaryDto>>
{
    // The requesting user (resolved from their token). Scopes the result to their own posts.
    public string AuthorId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
