using Application.Common.Interfaces.Repositories;
using Application.Features.Posts.Queries.GetMyPosts;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Tests.Application.Features.Posts;

public class GetMyPostsQueryHandlerTests
{
    private const string AuthorId = "author-1";

    private static Post PostWith(string id, PostStatus status) => new()
    {
        Id = id,
        Title = $"Title {id}",
        AuthorId = AuthorId,
        AuthorUsername = "aziz",
        Status = status
    };

    // Full-field fake post so mapping tests exercise every PostSummaryDto property,
    // not just Id/Status. Mirrors what a real Mongo-hydrated Post looks like.
    private static Post FullPost() => new()
    {
        Id = "p1",
        Title = "Understanding CQRS",
        Content = "Full markdown body...",
        Summary = "A short excerpt about CQRS.",
        AuthorId = AuthorId,
        AuthorUsername = "aziz",
        CreatedAt = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 1, 6, 9, 30, 0, DateTimeKind.Utc),
        PublishedAt = new DateTime(2026, 1, 7, 8, 0, 0, DateTimeKind.Utc),
        Status = PostStatus.Published,
        LikeCount = 42,
        CommentCount = 7
    };

    // The author's OWN posts of every status come back — including Pending and Rejected,
    // which the public feed hides. This is the whole point of the "My Posts" view.
    [Fact]
    public async Task Handle_ReturnsAuthorsPostsOfAllStatuses_MappedToSummaries()
    {
        var posts = new List<Post>
        {
            PostWith("p1", PostStatus.Pending),
            PostWith("p2", PostStatus.Published),
            PostWith("p3", PostStatus.Rejected)
        };
        var postRepo = Substitute.For<IPostRepository>();
        postRepo.GetByAuthorAsync(AuthorId, Arg.Any<int>(), Arg.Any<int>()).Returns(posts);

        var handler = new GetMyPostsQueryHandler(postRepo);

        var result = await handler.Handle(
            new GetMyPostsQuery { AuthorId = AuthorId, Page = 1, PageSize = 10 }, default);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, r => r.Id == "p1" && r.Status == PostStatus.Pending.ToString());
        Assert.Contains(result, r => r.Id == "p2" && r.Status == PostStatus.Published.ToString());
        Assert.Contains(result, r => r.Id == "p3" && r.Status == PostStatus.Rejected.ToString());

        // The repository is queried scoped to the requesting author.
        await postRepo.Received(1).GetByAuthorAsync(AuthorId, Arg.Any<int>(), Arg.Any<int>());
    }

    // Guards the DTO mapping itself: every PostSummaryDto field must round-trip from
    // the entity untouched, not just the couple of fields the other tests happen to check.
    [Fact]
    public async Task Handle_MapsEveryFieldOntoSummaryDto()
    {
        var post = FullPost();
        var postRepo = Substitute.For<IPostRepository>();
        postRepo.GetByAuthorAsync(AuthorId, Arg.Any<int>(), Arg.Any<int>()).Returns([post]);

        var handler = new GetMyPostsQueryHandler(postRepo);

        var result = await handler.Handle(
            new GetMyPostsQuery { AuthorId = AuthorId, Page = 1, PageSize = 10 }, default);

        var dto = Assert.Single(result);
        Assert.Equal(post.Id, dto.Id);
        Assert.Equal(post.Title, dto.Title);
        Assert.Equal(post.Summary, dto.Summary);
        Assert.Equal(post.AuthorId, dto.AuthorId);
        Assert.Equal(post.AuthorUsername, dto.AuthorUsername);
        Assert.Equal(post.CreatedAt, dto.CreatedAt);
        Assert.Equal(post.UpdatedAt, dto.UpdatedAt);
        Assert.Equal(post.PublishedAt, dto.PublishedAt);
        Assert.Equal(post.Status.ToString(), dto.Status);
        Assert.Equal(post.LikeCount, dto.LikeCount);
        Assert.Equal(post.CommentCount, dto.CommentCount);
    }

    // An author with no posts yet (or an out-of-range page) gets an empty list back,
    // not a null or an exception — the dashboard view must render an empty state cleanly.
    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenAuthorHasNoPosts()
    {
        var postRepo = Substitute.For<IPostRepository>();
        postRepo.GetByAuthorAsync(AuthorId, Arg.Any<int>(), Arg.Any<int>()).Returns([]);

        var handler = new GetMyPostsQueryHandler(postRepo);

        var result = await handler.Handle(
            new GetMyPostsQuery { AuthorId = AuthorId, Page = 1, PageSize = 10 }, default);

        Assert.Empty(result);
    }

    // The handler must pass the caller's AuthorId through verbatim — never a different
    // author's id — since this is the only thing that scopes the query to "my" posts.
    [Theory]
    [InlineData("author-1")]
    [InlineData("author-2")]
    [InlineData("64f1b2e9c2a4a7e1d8f4a111")]
    public async Task Handle_ScopesQueryToTheRequestingAuthorId(string authorId)
    {
        var postRepo = Substitute.For<IPostRepository>();
        postRepo.GetByAuthorAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>()).Returns([]);

        var handler = new GetMyPostsQueryHandler(postRepo);

        await handler.Handle(
            new GetMyPostsQuery { AuthorId = authorId, Page = 1, PageSize = 10 }, default);

        await postRepo.Received(1).GetByAuthorAsync(authorId, Arg.Any<int>(), Arg.Any<int>());
    }

    // Out-of-range paging is clamped to safe defaults before hitting the repository
    // (mirrors GetPostsQueryHandler): page floored to 1, pageSize to 10.
    // Boundary rows (1/100) prove the valid range is left untouched; only values
    // strictly outside [1, 100] get replaced.
    [Theory]
    [InlineData(0, 0, 1, 10)]
    [InlineData(-5, 500, 1, 10)]
    [InlineData(3, 25, 3, 25)]
    [InlineData(1, 1, 1, 1)]
    [InlineData(1, 100, 1, 100)]
    [InlineData(1, 101, 1, 10)]
    [InlineData(int.MinValue, int.MaxValue, 1, 10)]
    public async Task Handle_ClampsPageAndPageSize(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var postRepo = Substitute.For<IPostRepository>();
        postRepo.GetByAuthorAsync(AuthorId, Arg.Any<int>(), Arg.Any<int>()).Returns([]);

        var handler = new GetMyPostsQueryHandler(postRepo);

        await handler.Handle(
            new GetMyPostsQuery { AuthorId = AuthorId, Page = page, PageSize = pageSize }, default);

        await postRepo.Received(1).GetByAuthorAsync(AuthorId, expectedPage, expectedPageSize);
    }

    // The handler must not reorder what the repository returns — ordering (newest-first)
    // is the repository's responsibility (see IPostRepository.GetByAuthorAsync).
    [Fact]
    public async Task Handle_PreservesRepositoryOrder()
    {
        var posts = new List<Post>
        {
            PostWith("newest", PostStatus.Published),
            PostWith("middle", PostStatus.Pending),
            PostWith("oldest", PostStatus.Rejected)
        };
        var postRepo = Substitute.For<IPostRepository>();
        postRepo.GetByAuthorAsync(AuthorId, Arg.Any<int>(), Arg.Any<int>()).Returns(posts);

        var handler = new GetMyPostsQueryHandler(postRepo);

        var result = await handler.Handle(
            new GetMyPostsQuery { AuthorId = AuthorId, Page = 1, PageSize = 10 }, default);

        Assert.Equal(["newest", "middle", "oldest"], result.Select(r => r.Id));
    }
}
