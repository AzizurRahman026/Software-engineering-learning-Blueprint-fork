using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Blog.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IBlogCommentRepository _blogCommentRepository;

    public DeleteCommentCommandHandler(
        IBlogPostRepository blogPostRepository,
        IBlogCommentRepository blogCommentRepository)
    {
        _blogPostRepository = blogPostRepository;
        _blogCommentRepository = blogCommentRepository;
    }

    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _blogCommentRepository.GetByIdAsync<BlogComment>(request.CommentId)
            ?? throw new NotFoundException("Comment not found.");

        if (comment.AuthorId != request.UserId)
            throw new ValidationException("You can only delete your own comments.");

        await _blogCommentRepository.DeleteByIdAsync<BlogComment>(comment.Id);

        var post = await _blogPostRepository.GetByIdAsync<BlogPost>(comment.BlogPostId);
        if (post is not null && post.CommentCount > 0)
        {
            post.CommentCount -= 1;
            await _blogPostRepository.UpdateAsync(post);
        }
    }
}
