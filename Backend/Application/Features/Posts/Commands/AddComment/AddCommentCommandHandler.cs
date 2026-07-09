using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Features.Posts.DTOs;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Posts.Commands.AddComment;

public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, CommentDto>
{
    private readonly IPostRepository _blogPostRepository;
    private readonly IPostCommentRepository _blogCommentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cache;

    public AddCommentCommandHandler(
        IPostRepository blogPostRepository,
        IPostCommentRepository blogCommentRepository,
        IUserRepository userRepository,
        ICacheService cache)
    {
        _blogPostRepository = blogPostRepository;
        _blogCommentRepository = blogCommentRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<CommentDto> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Comment content is required.");

        var post = await _blogPostRepository.GetByIdAsync<Post>(request.PostId)
            ?? throw new NotFoundException("Post not found.");

        var author = await _userRepository.GetByIdAsync<User>(request.UserId)
            ?? throw new NotFoundException("User not found.");

        var comment = new PostComment
        {
            PostId = post.Id,
            AuthorId = author.Id,
            AuthorUsername = author.Username,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _blogCommentRepository.AddAsync(comment);

        post.CommentCount += 1;
        await _blogPostRepository.UpdateAsync(post);

        // The cached snapshot embeds the comment list + count — evict it.
        await _cache.RemoveAsync(PostCacheKeys.Detail(post.Id), cancellationToken);

        return CommentDto.FromEntity(comment);
    }
}
