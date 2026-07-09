using Domain.Entities;

namespace Application.Features.Posts.DTOs;

public class CommentDto
{
    public string Id { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static CommentDto FromEntity(PostComment c) => new()
    {
        Id = c.Id,
        PostId = c.PostId,
        AuthorId = c.AuthorId,
        AuthorUsername = c.AuthorUsername,
        Content = c.Content,
        CreatedAt = c.CreatedAt
    };
}
