namespace Domain.Entities;

public class PostComment : BaseEntity
{
    public string PostId { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
