using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand>
{
    private const int SummaryLength = 200;

    private readonly IPostRepository _blogPostRepository;
    private readonly ICacheService _cache;

    public UpdatePostCommandHandler(IPostRepository blogPostRepository, ICacheService cache)
    {
        _blogPostRepository = blogPostRepository;
        _cache = cache;
    }

    public async Task Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Content is required.");

        var post = await _blogPostRepository.GetByIdAsync<Post>(request.Id)
            ?? throw new NotFoundException("Post not found.");

        if (post.AuthorId != request.UserId)
            throw new ValidationException("You can only edit your own posts.");

        post.Title = request.Title.Trim();
        post.Content = request.Content;
        post.Summary = BuildSummary(request.Summary, request.Content);
        post.UpdatedAt = DateTime.UtcNow;

        await _blogPostRepository.UpdateAsync(post);

        // The cached detail snapshot is now stale — evict it so the next read repopulates.
        await _cache.RemoveAsync(PostCacheKeys.Detail(post.Id), cancellationToken);
    }

    private static string BuildSummary(string? summary, string content)
    {
        if (!string.IsNullOrWhiteSpace(summary))
            return summary.Trim();

        var text = content.Trim();
        return text.Length <= SummaryLength ? text : text[..SummaryLength] + "…";
    }
}
