using Domain.Entities;

namespace Application.Features.Blog.DTOs;

public class CommentDto
{
    public string Id { get; set; } = string.Empty;
    public string BlogPostId { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static CommentDto FromEntity(BlogComment c) => new()
    {
        Id = c.Id,
        BlogPostId = c.BlogPostId,
        AuthorId = c.AuthorId,
        AuthorUsername = c.AuthorUsername,
        Content = c.Content,
        CreatedAt = c.CreatedAt
    };
}
