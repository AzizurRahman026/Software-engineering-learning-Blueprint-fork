using Application.Features.Blog.DTOs;
using MediatR;

namespace Application.Features.Blog.Commands.AddComment;

public class AddCommentCommand : IRequest<CommentDto>
{
    public string BlogPostId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
