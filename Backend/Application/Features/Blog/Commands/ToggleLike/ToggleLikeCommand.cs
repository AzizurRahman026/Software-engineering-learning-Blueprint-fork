using Application.Features.Blog.DTOs;
using MediatR;

namespace Application.Features.Blog.Commands.ToggleLike;

public class ToggleLikeCommand : IRequest<ToggleLikeResponseDto>
{
    public string BlogPostId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
