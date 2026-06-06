using MediatR;

namespace Application.Features.Blog.Commands.CreateBlogPost;

public class CreateBlogPostCommand : IRequest<string>
{
    public string AuthorId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
}
