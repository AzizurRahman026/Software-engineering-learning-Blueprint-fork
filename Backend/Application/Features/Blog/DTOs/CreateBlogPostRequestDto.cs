using Application.Features.Blog.Commands.CreateBlogPost;

namespace Application.Features.Blog.DTOs;

public class CreateBlogPostRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }

    public CreateBlogPostCommand ToCreateBlogPostCommand(string authorId) => new()
    {
        AuthorId = authorId,
        Title = Title,
        Content = Content,
        Summary = Summary
    };
}
