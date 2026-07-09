using Application.Features.Posts.DTOs;
using MediatR;

namespace Application.Features.Posts.Commands.AddComment;

public class AddCommentCommand : IRequest<CommentDto>
{
    public string PostId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
