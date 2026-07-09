using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
{
    private readonly IPostRepository _blogPostRepository;
    private readonly IPostCommentRepository _blogCommentRepository;
    private readonly IPostLikeRepository _blogLikeRepository;
    private readonly ICacheService _cache;

    public DeletePostCommandHandler(
        IPostRepository blogPostRepository,
        IPostCommentRepository blogCommentRepository,
        IPostLikeRepository blogLikeRepository,
        ICacheService cache)
    {
        _blogPostRepository = blogPostRepository;
        _blogCommentRepository = blogCommentRepository;
        _blogLikeRepository = blogLikeRepository;
        _cache = cache;
    }

    public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _blogPostRepository.GetByIdAsync<Post>(request.Id)
            ?? throw new NotFoundException("Post not found.");

        if (post.AuthorId != request.UserId)
            throw new ValidationException("You can only delete your own posts.");

        // Cascade: remove the post's comments and likes so we don't leave orphans.
        var comments = await _blogCommentRepository.GetByPostIdAsync(post.Id);
        foreach (var comment in comments)
            await _blogCommentRepository.DeleteByIdAsync<PostComment>(comment.Id);

        var likes = await _blogLikeRepository.GetByPostIdAsync(post.Id);
        foreach (var like in likes)
            await _blogLikeRepository.DeleteByIdAsync<PostLike>(like.Id);

        await _blogPostRepository.DeleteByIdAsync<Post>(post.Id);

        // The post is gone — drop its cached snapshot so a miss returns NotFound, not stale data.
        await _cache.RemoveAsync(PostCacheKeys.Detail(post.Id), cancellationToken);
    }
}
