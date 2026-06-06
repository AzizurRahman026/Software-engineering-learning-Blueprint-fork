namespace Domain.Entities;

// One row per (post, user). Uniqueness is enforced in the ToggleLike handler.
public class BlogLike : BaseEntity
{
    public string BlogPostId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
