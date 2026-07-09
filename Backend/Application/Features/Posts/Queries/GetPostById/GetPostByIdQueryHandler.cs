using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Features.Posts.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Posts.Queries.GetPostById;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDetailDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostCommentRepository _postCommentRepository;
    private readonly IPostLikeRepository _postLikeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cache;

    // Short TTL bounds staleness; write handlers also evict via PostCacheKeys.Detail.
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    public GetPostByIdQueryHandler(
        IPostRepository postRepository,
        IPostCommentRepository postCommentRepository,
        IPostLikeRepository postLikeRepository,
        IUserRepository userRepository,
        ICacheService cache)
    {
        _postRepository = postRepository;
        _postCommentRepository = postCommentRepository;
        _postLikeRepository = postLikeRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<PostDetailDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var key = PostCacheKeys.Detail(request.Id);

        // 1. Try cache. Only Published posts are ever cached, so a hit is always publicly viewable.
        var dto = await _cache.GetAsync<PostDetailDto>(key, cancellationToken);

        // 2. Miss -> read from Mongo.
        if (dto is null)
        {
            var post = await _postRepository.GetByIdAsync<Post>(request.Id)
                ?? throw new NotFoundException("Post not found.");

            // Non-published posts are visible only to their author or an admin. Others get a 404
            // (rather than 403) so the endpoint can't confirm a pending/rejected post exists.
            if (post.Status != PostStatus.Published && !await CanViewUnpublishedAsync(post, request.UserId))
                throw new NotFoundException("Post not found.");

            var comments = await _postCommentRepository.GetByPostIdAsync(post.Id);
            dto = PostDetailDto.FromEntity(post, comments, likedByCurrentUser: false);

            // Only cache published posts, so the public detail cache never serves gated content.
            if (post.Status == PostStatus.Published)
                await _cache.SetAsync(key, dto, CacheTtl, cancellationToken);
        }

        // 3. Overlay the per-user like fresh — this part is never cached.
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var like = await _postLikeRepository.GetByPostAndUserAsync(dto.Id, request.UserId);
            dto.LikedByCurrentUser = like is not null;
        }

        return dto;
    }

    private async Task<bool> CanViewUnpublishedAsync(Post post, string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return false;
        if (post.AuthorId == userId) return true;

        var user = await _userRepository.GetByIdAsync<User>(userId);
        return user is not null && user.Role is UserRole.Admin or UserRole.SuperAdmin;
    }
}
