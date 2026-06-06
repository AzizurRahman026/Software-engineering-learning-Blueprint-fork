using MediatR;

namespace Application.Features.Blog.Commands.DeleteComment;

public class DeleteCommentCommand : IRequest
{
    public string BlogPostId { get; set; } = string.Empty;
    public string CommentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
