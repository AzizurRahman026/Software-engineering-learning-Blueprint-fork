using Domain.Entities;

namespace Application.Features.Posts.DTOs;

public class PostSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }

    public static PostSummaryDto FromEntity(Post p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Summary = p.Summary,
        AuthorId = p.AuthorId,
        AuthorUsername = p.AuthorUsername,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        PublishedAt = p.PublishedAt,
        Status = p.Status.ToString(),
        LikeCount = p.LikeCount,
        CommentCount = p.CommentCount
    };
}
