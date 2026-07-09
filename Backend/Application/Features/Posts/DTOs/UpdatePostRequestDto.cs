using Application.Features.Posts.Commands.UpdatePost;

namespace Application.Features.Posts.DTOs;

public class UpdatePostRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }

    public UpdatePostCommand ToUpdatePostCommand(string id, string userId) => new()
    {
        Id = id,
        UserId = userId,
        Title = Title,
        Content = Content,
        Summary = Summary
    };
}
