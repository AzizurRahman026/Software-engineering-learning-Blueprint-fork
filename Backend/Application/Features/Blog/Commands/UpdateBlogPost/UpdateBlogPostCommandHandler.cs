using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Blog.Commands.UpdateBlogPost;

public class UpdateBlogPostCommandHandler : IRequestHandler<UpdateBlogPostCommand>
{
    private const int SummaryLength = 200;

    private readonly IBlogPostRepository _blogPostRepository;

    public UpdateBlogPostCommandHandler(IBlogPostRepository blogPostRepository)
    {
        _blogPostRepository = blogPostRepository;
    }

    public async Task Handle(UpdateBlogPostCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Content is required.");

        var post = await _blogPostRepository.GetByIdAsync<BlogPost>(request.Id)
            ?? throw new NotFoundException("Blog post not found.");

        if (post.AuthorId != request.UserId)
            throw new ValidationException("You can only edit your own posts.");

        post.Title = request.Title.Trim();
        post.Content = request.Content;
        post.Summary = BuildSummary(request.Summary, request.Content);
        post.UpdatedAt = DateTime.UtcNow;

        await _blogPostRepository.UpdateAsync(post);
    }

    private static string BuildSummary(string? summary, string content)
    {
        if (!string.IsNullOrWhiteSpace(summary))
            return summary.Trim();

        var text = content.Trim();
        return text.Length <= SummaryLength ? text : text[..SummaryLength] + "…";
    }
}
