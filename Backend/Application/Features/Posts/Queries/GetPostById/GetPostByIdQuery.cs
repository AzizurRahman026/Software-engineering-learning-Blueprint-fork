using Application.Features.Posts.DTOs;
using MediatR;

namespace Application.Features.Posts.Queries.GetPostById;

public class GetPostByIdQuery : IRequest<PostDetailDto>
{
    public string Id { get; set; } = string.Empty;

    // Optional: present when the reader is logged in, used to compute LikedByCurrentUser.
    public string? UserId { get; set; }
}
