using MediatR;

namespace Application.Features.Posts.Commands.PublishPost;

public class PublishPostCommand : IRequest
{
    public string PostId { get; set; } = string.Empty;
    // The admin performing the action (from X-User-Id).
    public string ActingUserId { get; set; } = string.Empty;
}
