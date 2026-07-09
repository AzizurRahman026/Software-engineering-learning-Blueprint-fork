using Application.Common.Interfaces.Publisher;
using Application.Features.Posts.Commands.AddComment;
using Application.Features.Posts.Commands.CreatePost;
using Application.Features.Posts.Commands.DeletePost;
using Application.Features.Posts.Commands.DeleteComment;
using Application.Features.Posts.Commands.PublishPost;
using Application.Features.Posts.Commands.RejectPost;
using Application.Features.Posts.Commands.ToggleLike;
using Application.Features.Posts.Commands.UpdatePost;
using Application.Features.Posts.DTOs;
using Application.Features.Posts.Queries.GetPendingPosts;
using Application.Features.Posts.Queries.GetPostById;
using Application.Features.Posts.Queries.GetPosts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Writes/authored/admin actions require a token; public reads opt out with [AllowAnonymous].
public class PostsController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public PostsController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    // The authenticated user's id from the JWT 'sub' claim (null on anonymous requests).
    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // ── GET /api/posts?page=&pageSize= — public feed ──────────────────────────
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PostSummaryDto>>> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetPostsQuery { Page = page, PageSize = pageSize };
        var posts = await _messageBus.SendAsync<GetPostsQuery, List<PostSummaryDto>>(query);
        return Ok(posts);
    }

    // ── GET /api/posts/{id} — public; likedByCurrentUser filled if authenticated ──
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PostDetailDto>> GetPost(string id)
    {
        var query = new GetPostByIdQuery { Id = id, UserId = GetUserId() };
        var post = await _messageBus.SendAsync<GetPostByIdQuery, PostDetailDto>(query);
        return Ok(post);
    }

    // ── POST /api/posts — create (requires login) ─────────────────────────────
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> CreatePost([FromBody] CreatePostRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        var command = request.ToCreatePostCommand(userId);
        var postId = await _messageBus.SendAsync<CreatePostCommand, string>(command);
        if (string.IsNullOrEmpty(postId))
            return BadRequest("Failed to create post.");

        return Created($"/api/posts/{postId}", new { id = postId });
    }

    // ── PUT /api/posts/{id} — update own post ─────────────────────────────────
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePost(string id, [FromBody] UpdatePostRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        await _messageBus.SendAsync(request.ToUpdatePostCommand(id, userId));
        return Ok(new { message = "Post updated successfully." });
    }

    // ── DELETE /api/posts/{id} — delete own post ──────────────────────────────
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePost(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        await _messageBus.SendAsync(new DeletePostCommand { Id = id, UserId = userId });
        return NoContent();
    }

    // ── POST /api/posts/{id}/comments — add a comment ─────────────────────────
    [HttpPost("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<CommentDto>> AddComment(string id, [FromBody] AddCommentRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        var command = new AddCommentCommand { PostId = id, UserId = userId, Content = request.Content };
        var comment = await _messageBus.SendAsync<AddCommentCommand, CommentDto>(command);
        return Created($"/api/posts/{id}", comment);
    }

    // ── DELETE /api/posts/{id}/comments/{commentId} — delete own comment ──────
    [HttpDelete("{id}/comments/{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteComment(string id, string commentId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        await _messageBus.SendAsync(new DeleteCommentCommand { PostId = id, CommentId = commentId, UserId = userId });
        return NoContent();
    }

    // ── POST /api/posts/{id}/like — toggle like ───────────────────────────────
    [HttpPost("{id}/like")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ToggleLikeResponseDto>> ToggleLike(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        var result = await _messageBus.SendAsync<ToggleLikeCommand, ToggleLikeResponseDto>(
            new ToggleLikeCommand { PostId = id, UserId = userId });
        return Ok(result);
    }

    // ── GET /api/posts/pending — moderation queue (admin only) ────────────────
    [HttpGet("pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<PostSummaryDto>>> GetPending()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        var posts = await _messageBus.SendAsync<GetPendingPostsQuery, List<PostSummaryDto>>(
            new GetPendingPostsQuery { ActingUserId = userId });
        return Ok(posts);
    }

    // ── POST /api/posts/{id}/publish — approve + email subscribers (admin only) ─
    [HttpPost("{id}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Publish(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        await _messageBus.SendAsync(new PublishPostCommand { PostId = id, ActingUserId = userId });
        return Ok(new { message = "Post published." });
    }

    // ── POST /api/posts/{id}/reject — decline a pending post (admin only) ──────
    [HttpPost("{id}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Reject(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        await _messageBus.SendAsync(new RejectPostCommand { PostId = id, ActingUserId = userId });
        return Ok(new { message = "Post rejected." });
    }
}
