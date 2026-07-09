using Application.Features.Posts.DTOs;
using MediatR;

namespace Application.Features.Posts.Commands.ToggleLike;

public class ToggleLikeCommand : IRequest<ToggleLikeResponseDto>
{
    public string PostId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
