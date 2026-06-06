using Application.Features.Blog.DTOs;
using MediatR;

namespace Application.Features.Blog.Queries.GetBlogPostById;

public class GetBlogPostByIdQuery : IRequest<BlogPostDetailDto>
{
    public string Id { get; set; } = string.Empty;

    // Optional: present when the reader is logged in, used to compute LikedByCurrentUser.
    public string? UserId { get; set; }
}
