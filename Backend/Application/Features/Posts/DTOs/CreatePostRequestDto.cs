using Application.Features.Posts.Commands.CreatePost;

namespace Application.Features.Posts.DTOs;

public class CreatePostRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }

    public CreatePostCommand ToCreatePostCommand(string authorId) => new()
    {
        AuthorId = authorId,
        Title = Title,
        Content = Content,
        Summary = Summary
    };
}
