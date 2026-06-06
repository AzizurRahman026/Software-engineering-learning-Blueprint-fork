using Application.Features.Blog.Commands.UpdateBlogPost;

namespace Application.Features.Blog.DTOs;

public class UpdateBlogPostRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }

    public UpdateBlogPostCommand ToUpdateBlogPostCommand(string id, string userId) => new()
    {
        Id = id,
        UserId = userId,
        Title = Title,
        Content = Content,
        Summary = Summary
    };
}
