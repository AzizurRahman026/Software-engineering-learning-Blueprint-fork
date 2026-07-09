using MediatR;

namespace Application.Features.Posts.Commands.DeleteComment;

public class DeleteCommentCommand : IRequest
{
    public string PostId { get; set; } = string.Empty;
    public string CommentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
