using Application.Common.Interfaces.Publisher;
using Application.Features.Blog.Commands.AddComment;
using Application.Features.Blog.Commands.CreateBlogPost;
using Application.Features.Blog.Commands.DeleteBlogPost;
using Application.Features.Blog.Commands.DeleteComment;
using Application.Features.Blog.Commands.ToggleLike;
using Application.Features.Blog.Commands.UpdateBlogPost;
using Application.Features.Blog.DTOs;
using Application.Features.Blog.Queries.GetBlogPostById;
using Application.Features.Blog.Queries.GetBlogPosts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private const string UserIdHeader = "X-User-Id";

    private readonly IMessageBus _messageBus;

    public BlogController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    // Reads the logged-in user's id from the X-User-Id header (set by the Angular interceptor).
    private string? GetUserId() =>
        Request.Headers.TryGetValue(UserIdHeader, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : null;

    // ── GET /api/blog?page=&pageSize= — public feed ──────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BlogPostSummaryDto>>> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetBlogPostsQuery { Page = page, PageSize = pageSize };
        var posts = await _messageBus.SendAsync<GetBlogPostsQuery, List<BlogPostSummaryDto>>(query);
        return Ok(posts);
    }

    // ── GET /api/blog/{id} — public; likedByCurrentUser filled if header present ──
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<BlogPostDetailDto>> GetPost(string id)
    {
        var query = new GetBlogPostByIdQuery { Id = id, UserId = GetUserId() };
        var post = await _messageBus.SendAsync<GetBlogPostByIdQuery, BlogPostDetailDto>(query);
        return Ok(post);
    }

    // ── POST /api/blog — create (requires login) ─────────────────────────────
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> CreatePost([FromBody] CreateBlogPostRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        var command = request.ToCreateBlogPostCommand(userId);
        var postId = await _messageBus.SendAsync<CreateBlogPostCommand, string>(command);
        if (string.IsNullOrEmpty(postId))
            return BadRequest("Failed to create blog post.");

        return Created($"/api/blog/{postId}", new { id = postId });
    }

    // ── PUT /api/blog/{id} — update own post ─────────────────────────────────
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePost(string id, [FromBody] UpdateBlogPostRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        await _messageBus.SendAsync(request.ToUpdateBlogPostCommand(id, userId));
        return Ok(new { message = "Blog post updated successfully." });
    }

    // ── DELETE /api/blog/{id} — delete own post ──────────────────────────────
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePost(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        await _messageBus.SendAsync(new DeleteBlogPostCommand { Id = id, UserId = userId });
        return NoContent();
    }

    // ── POST /api/blog/{id}/comments — add a comment ─────────────────────────
    [HttpPost("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<CommentDto>> AddComment(string id, [FromBody] AddCommentRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        var command = new AddCommentCommand { BlogPostId = id, UserId = userId, Content = request.Content };
        var comment = await _messageBus.SendAsync<AddCommentCommand, CommentDto>(command);
        return Created($"/api/blog/{id}", comment);
    }

    // ── DELETE /api/blog/{id}/comments/{commentId} — delete own comment ──────
    [HttpDelete("{id}/comments/{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteComment(string id, string commentId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        await _messageBus.SendAsync(new DeleteCommentCommand { BlogPostId = id, CommentId = commentId, UserId = userId });
        return NoContent();
    }

    // ── POST /api/blog/{id}/like — toggle like ───────────────────────────────
    [HttpPost("{id}/like")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ToggleLikeResponseDto>> ToggleLike(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        var result = await _messageBus.SendAsync<ToggleLikeCommand, ToggleLikeResponseDto>(
            new ToggleLikeCommand { BlogPostId = id, UserId = userId });
        return Ok(result);
    }
}
