using MediatR;

namespace Application.Features.Blog.Commands.DeleteBlogPost;

public class DeleteBlogPostCommand : IRequest
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
