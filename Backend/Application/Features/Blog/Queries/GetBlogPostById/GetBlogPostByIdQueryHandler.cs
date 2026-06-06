using Application.Common.Interfaces.Repositories;
using Application.Features.Blog.DTOs;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Blog.Queries.GetBlogPostById;

public class GetBlogPostByIdQueryHandler : IRequestHandler<GetBlogPostByIdQuery, BlogPostDetailDto>
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IBlogCommentRepository _blogCommentRepository;
    private readonly IBlogLikeRepository _blogLikeRepository;

    public GetBlogPostByIdQueryHandler(
        IBlogPostRepository blogPostRepository,
        IBlogCommentRepository blogCommentRepository,
        IBlogLikeRepository blogLikeRepository)
    {
        _blogPostRepository = blogPostRepository;
        _blogCommentRepository = blogCommentRepository;
        _blogLikeRepository = blogLikeRepository;
    }

    public async Task<BlogPostDetailDto> Handle(GetBlogPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _blogPostRepository.GetByIdAsync<BlogPost>(request.Id)
            ?? throw new NotFoundException("Blog post not found.");

        var comments = await _blogCommentRepository.GetByPostIdAsync(post.Id);

        var likedByCurrentUser = false;
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var like = await _blogLikeRepository.GetByPostAndUserAsync(post.Id, request.UserId);
            likedByCurrentUser = like is not null;
        }

        return BlogPostDetailDto.FromEntity(post, comments, likedByCurrentUser);
    }
}
