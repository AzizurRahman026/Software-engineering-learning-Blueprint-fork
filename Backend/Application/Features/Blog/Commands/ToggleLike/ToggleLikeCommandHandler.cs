using Application.Common.Interfaces.Repositories;
using Application.Features.Blog.DTOs;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Blog.Commands.ToggleLike;

public class ToggleLikeCommandHandler : IRequestHandler<ToggleLikeCommand, ToggleLikeResponseDto>
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IBlogLikeRepository _blogLikeRepository;

    public ToggleLikeCommandHandler(
        IBlogPostRepository blogPostRepository,
        IBlogLikeRepository blogLikeRepository)
    {
        _blogPostRepository = blogPostRepository;
        _blogLikeRepository = blogLikeRepository;
    }

    public async Task<ToggleLikeResponseDto> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var post = await _blogPostRepository.GetByIdAsync<BlogPost>(request.BlogPostId)
            ?? throw new NotFoundException("Blog post not found.");

        var existing = await _blogLikeRepository.GetByPostAndUserAsync(post.Id, request.UserId);

        bool liked;
        if (existing is not null)
        {
            await _blogLikeRepository.DeleteByIdAsync<BlogLike>(existing.Id);
            post.LikeCount = post.LikeCount > 0 ? post.LikeCount - 1 : 0;
            liked = false;
        }
        else
        {
            await _blogLikeRepository.AddAsync(new BlogLike
            {
                BlogPostId = post.Id,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            });
            post.LikeCount += 1;
            liked = true;
        }

        await _blogPostRepository.UpdateAsync(post);

        return new ToggleLikeResponseDto
        {
            Liked = liked,
            LikeCount = post.LikeCount
        };
    }
}
