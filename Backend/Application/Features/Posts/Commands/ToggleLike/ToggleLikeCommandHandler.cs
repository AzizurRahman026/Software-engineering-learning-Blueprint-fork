using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Features.Posts.DTOs;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Posts.Commands.ToggleLike;

public class ToggleLikeCommandHandler : IRequestHandler<ToggleLikeCommand, ToggleLikeResponseDto>
{
    private readonly IPostRepository _blogPostRepository;
    private readonly IPostLikeRepository _blogLikeRepository;
    private readonly ICacheService _cache;

    public ToggleLikeCommandHandler(
        IPostRepository blogPostRepository,
        IPostLikeRepository blogLikeRepository,
        ICacheService cache)
    {
        _blogPostRepository = blogPostRepository;
        _blogLikeRepository = blogLikeRepository;
        _cache = cache;
    }

    public async Task<ToggleLikeResponseDto> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var post = await _blogPostRepository.GetByIdAsync<Post>(request.PostId)
            ?? throw new NotFoundException("Post not found.");

        var existing = await _blogLikeRepository.GetByPostAndUserAsync(post.Id, request.UserId);

        bool liked;
        if (existing is not null)
        {
            await _blogLikeRepository.DeleteByIdAsync<PostLike>(existing.Id);
            post.LikeCount = post.LikeCount > 0 ? post.LikeCount - 1 : 0;
            liked = false;
        }
        else
        {
            await _blogLikeRepository.AddAsync(new PostLike
            {
                PostId = post.Id,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            });
            post.LikeCount += 1;
            liked = true;
        }

        await _blogPostRepository.UpdateAsync(post);

        // LikeCount changed in the cached snapshot — evict it. (The per-user like flag
        // is overlaid fresh after the cache read, so only the count needs invalidation.)
        await _cache.RemoveAsync(PostCacheKeys.Detail(post.Id), cancellationToken);

        return new ToggleLikeResponseDto
        {
            Liked = liked,
            LikeCount = post.LikeCount
        };
    }
}
