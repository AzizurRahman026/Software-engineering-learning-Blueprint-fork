using MediatR;

namespace Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommand : IRequest<string>
{
    public string AuthorId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
}
