using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Features.Posts;
using Application.Features.Posts.Commands.AddComment;
using Application.Features.Posts.DTOs;
using Application.Features.Posts.Queries.GetPostById;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using NSubstitute;

namespace Tests.Application.Features.Posts;

public class PostCacheAsideTests
{
    private const string PostId = "post-123";

    // ── 1. READ: a cache HIT is served from cache, the repository is never touched ──────────
    [Fact]
    public async Task GetPostById_OnCacheHit_ReturnsSnapshotWithoutQueryingRepository()
    {
        // Arrange — pre-seed the cache so the handler finds the snapshot immediately.
        var key = PostCacheKeys.Detail(PostId);
        var cache = Substitute.For<ICacheService>();
        cache.GetAsync<PostDetailDto>(key, Arg.Any<CancellationToken>())
             .Returns(new PostDetailDto { Id = PostId, Title = "Cached title" });

        var postRepo = Substitute.For<IPostRepository>();
        var commentRepo = Substitute.For<IPostCommentRepository>();
        var handler = new GetPostByIdQueryHandler(
            postRepo, commentRepo, Substitute.For<IPostLikeRepository>(), Substitute.For<IUserRepository>(), cache);

        // Act
        var result = await handler.Handle(new GetPostByIdQuery { Id = PostId }, default);

        // Assert — the cached snapshot is returned verbatim …
        Assert.Equal(PostId, result.Id);
        Assert.Equal("Cached title", result.Title);
        // … Mongo is never touched, comments are not re-read, and nothing is re-cached.
        await postRepo.DidNotReceive().GetByIdAsync<Post>(Arg.Any<string>());
        await commentRepo.DidNotReceive().GetByPostIdAsync(Arg.Any<string>());
        await cache.DidNotReceive().SetAsync(
            Arg.Any<string>(), Arg.Any<PostDetailDto>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    // ── 2. READ: a cache MISS reads Mongo, builds the DTO, and populates the cache ──────────
    [Fact]
    public async Task GetPostById_OnCacheMiss_ReadsRepositoryAndCachesTheBuiltSnapshot()
    {
        var key = PostCacheKeys.Detail(PostId);
        var cache = Substitute.For<ICacheService>(); // GetAsync returns null by default -> miss

        var post = new Post
        {
            Id = PostId,
            Title = "From Mongo",
            Content = "body",
            AuthorUsername = "aziz",
            CommentCount = 1,
            LikeCount = 5,
            // Published posts are the ones served + cached on the public detail path.
            Status = PostStatus.Published
        };
        var postRepo = Substitute.For<IPostRepository>();
        postRepo.GetByIdAsync<Post>(PostId).Returns(post);

        var comments = new List<PostComment>
        {
            new() { Id = "c1", PostId = PostId, AuthorUsername = "bob", Content = "first comment" }
        };
        var commentRepo = Substitute.For<IPostCommentRepository>();
        commentRepo.GetByPostIdAsync(PostId).Returns(comments);

        var handler = new GetPostByIdQueryHandler(
            postRepo, commentRepo, Substitute.For<IPostLikeRepository>(), Substitute.For<IUserRepository>(), cache);

        var result = await handler.Handle(new GetPostByIdQuery { Id = PostId }, default);

        // Result is the snapshot built from the entity + its comments.
        Assert.Equal(PostId, result.Id);
        Assert.Equal("From Mongo", result.Title);
        Assert.Equal("body", result.Content);
        Assert.Equal(5, result.LikeCount);
        Assert.Equal(1, result.CommentCount);
        var comment = Assert.Single(result.Comments);
        Assert.Equal("first comment", comment.Content);
        Assert.Equal("bob", comment.AuthorUsername);

        // The read fell through to Mongo for both the post and its comments …
        await postRepo.Received(1).GetByIdAsync<Post>(PostId);
        await commentRepo.Received(1).GetByPostIdAsync(PostId);

        // … and the SAME snapshot was cached under the read-path key with a positive TTL.
        await cache.Received(1).SetAsync(
            key,
            Arg.Is<PostDetailDto>(d => d.Id == PostId && d.Title == "From Mongo" && d.Comments.Count == 1),
            Arg.Is<TimeSpan>(ttl => ttl > TimeSpan.Zero),
            Arg.Any<CancellationToken>());
    }

    // ── 3. WRITE: AddComment persists the comment, bumps the count, and evicts the cache ────
    [Fact]
    public async Task AddComment_PersistsComment_IncrementsCount_AndEvictsDetailCacheKey()
    {
        var cache = Substitute.For<ICacheService>();

        var post = new Post { Id = PostId, CommentCount = 4 };
        var postRepo = Substitute.For<IPostRepository>();
        postRepo.GetByIdAsync<Post>(PostId).Returns(post);

        var userRepo = Substitute.For<IUserRepository>();
        var user = User.Register("aziz", Email.Create("aziz@example.com"), "hash", "salt");
        user.Id = "user-1";
        userRepo.GetByIdAsync<User>("user-1").Returns(user);

        var commentRepo = Substitute.For<IPostCommentRepository>();
        var handler = new AddCommentCommandHandler(postRepo, commentRepo, userRepo, cache);

        // Untrimmed content verifies the handler trims before persisting.
        var result = await handler.Handle(
            new AddCommentCommand { PostId = PostId, UserId = "user-1", Content = "  nice post  " },
            default);

        // Returned DTO reflects the new comment, stamped with the resolved author and trimmed content.
        Assert.Equal(PostId, result.PostId);
        Assert.Equal("user-1", result.AuthorId);
        Assert.Equal("aziz", result.AuthorUsername);
        Assert.Equal("nice post", result.Content);

        // The comment was persisted with those same fields …
        await commentRepo.Received(1).AddAsync(Arg.Is<PostComment>(c =>
            c.PostId == PostId &&
            c.AuthorId == "user-1" &&
            c.AuthorUsername == "aziz" &&
            c.Content == "nice post"));

        // … the post's comment count was incremented and saved …
        await postRepo.Received(1).UpdateAsync(Arg.Is<Post>(p => p.Id == PostId && p.CommentCount == 5));

        // … and the cached snapshot was evicted under the SAME key the read path uses.
        await cache.Received(1).RemoveAsync(PostCacheKeys.Detail(PostId), Arg.Any<CancellationToken>());
    }
}
