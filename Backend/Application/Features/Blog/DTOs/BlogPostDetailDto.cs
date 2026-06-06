using Domain.Entities;

namespace Application.Features.Blog.DTOs;

public class BlogPostDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool LikedByCurrentUser { get; set; }
    public List<CommentDto> Comments { get; set; } = new();

    public static BlogPostDetailDto FromEntity(BlogPost p, List<BlogComment> comments, bool likedByCurrentUser) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Content = p.Content,
        Summary = p.Summary,
        AuthorId = p.AuthorId,
        AuthorUsername = p.AuthorUsername,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        LikeCount = p.LikeCount,
        CommentCount = p.CommentCount,
        LikedByCurrentUser = likedByCurrentUser,
        Comments = comments
            .OrderBy(c => c.CreatedAt)
            .Select(CommentDto.FromEntity)
            .ToList()
    };
}
