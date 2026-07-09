using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Posts.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly IPostRepository _blogPostRepository;
    private readonly IPostCommentRepository _blogCommentRepository;
    private readonly ICacheService _cache;

    public DeleteCommentCommandHandler(
        IPostRepository blogPostRepository,
        IPostCommentRepository blogCommentRepository,
        ICacheService cache)
    {
        _blogPostRepository = blogPostRepository;
        _blogCommentRepository = blogCommentRepository;
        _cache = cache;
    }

    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _blogCommentRepository.GetByIdAsync<PostComment>(request.CommentId)
            ?? throw new NotFoundException("Comment not found.");

        if (comment.AuthorId != request.UserId)
            throw new ValidationException("You can only delete your own comments.");

        await _blogCommentRepository.DeleteByIdAsync<PostComment>(comment.Id);

        var post = await _blogPostRepository.GetByIdAsync<Post>(comment.PostId);
        if (post is not null && post.CommentCount > 0)
        {
            post.CommentCount -= 1;
            await _blogPostRepository.UpdateAsync(post);
        }

        // The deleted comment lives inside the cached snapshot — evict it.
        await _cache.RemoveAsync(PostCacheKeys.Detail(comment.PostId), cancellationToken);
    }
}
