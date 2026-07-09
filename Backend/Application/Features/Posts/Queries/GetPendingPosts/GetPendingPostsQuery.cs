using Application.Features.Posts.DTOs;
using MediatR;

namespace Application.Features.Posts.Queries.GetPendingPosts;

public class GetPendingPostsQuery : IRequest<List<PostSummaryDto>>
{
    // The requesting user (from X-User-Id). Must be Admin or SuperAdmin.
    public string ActingUserId { get; set; } = string.Empty;
}
