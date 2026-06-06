using Domain.Entities;

namespace Application.Features.Blog.DTOs;

public class BlogPostSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }

    public static BlogPostSummaryDto FromEntity(BlogPost p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Summary = p.Summary,
        AuthorId = p.AuthorId,
        AuthorUsername = p.AuthorUsername,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        LikeCount = p.LikeCount,
        CommentCount = p.CommentCount
    };
}
