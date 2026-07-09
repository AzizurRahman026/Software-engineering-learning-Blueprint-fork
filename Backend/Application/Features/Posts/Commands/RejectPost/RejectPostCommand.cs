using MediatR;

namespace Application.Features.Posts.Commands.RejectPost;

public class RejectPostCommand : IRequest
{
    public string PostId { get; set; } = string.Empty;
    // The admin performing the action (from X-User-Id).
    public string ActingUserId { get; set; } = string.Empty;
}
