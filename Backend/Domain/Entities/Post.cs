using Domain.Enums;

namespace Domain.Entities;

public class Post : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    // Raw Markdown authored by the user; rendered on the client with `marked`.
    public string Content { get; set; } = string.Empty;

    // Short excerpt shown in the list view so we don't ship full content for every card.
    public string Summary { get; set; } = string.Empty;

    public string AuthorId { get; set; } = string.Empty;

    // Denormalized so the list view can show the author without a per-post user lookup.
    public string AuthorUsername { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Moderation workflow: new posts start Pending; an admin publishes (→ newsletter) or rejects.
    public PostStatus Status { get; set; } = PostStatus.Pending;
    public DateTime? PublishedAt { get; set; }

    // Denormalized counters kept in sync by the comment/like handlers.
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
}
