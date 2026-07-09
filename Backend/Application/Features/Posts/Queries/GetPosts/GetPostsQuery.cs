using Application.Features.Posts.DTOs;
using MediatR;

namespace Application.Features.Posts.Queries.GetPosts;

public class GetPostsQuery : IRequest<List<PostSummaryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
