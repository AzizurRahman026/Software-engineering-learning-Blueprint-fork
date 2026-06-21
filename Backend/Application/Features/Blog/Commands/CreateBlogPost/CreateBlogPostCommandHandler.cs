using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Blog.Commands.CreateBlogPost;

public class CreateBlogPostCommandHandler : IRequestHandler<CreateBlogPostCommand, string>
{
    private const int SummaryLength = 200;

    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IUserRepository _userRepository;

    public CreateBlogPostCommandHandler(IBlogPostRepository blogPostRepository, IUserRepository userRepository)
    {
        _blogPostRepository = blogPostRepository;
        _userRepository = userRepository;
    }

    public async Task<string> Handle(CreateBlogPostCommand request, CancellationToken cancellationToken)
    {
        // Validation moved to CreateBlogPostCommandValidator and runs in the pipeline before this handler
        var author = await _userRepository.GetByIdAsync<User>(request.AuthorId);
        if (author is null)
            throw new NotFoundException("Author not found.");

        var post = new BlogPost
        {
            Title = request.Title.Trim(),
            Content = request.Content,
            Summary = BuildSummary(request.Summary, request.Content),
            AuthorId = author.Id,
            AuthorUsername = author.Username,
            CreatedAt = DateTime.UtcNow
        };

        var added = await _blogPostRepository.AddAsync(post);
        if (!added) return string.Empty;
        return post.Id;
    }

    // Use the author-supplied summary if present, otherwise derive a plain-text excerpt from the content.
    private static string BuildSummary(string? summary, string content)
    {
        if (!string.IsNullOrWhiteSpace(summary))
            return summary.Trim();

        var text = content.Trim();
        return text.Length <= SummaryLength ? text : text[..SummaryLength] + "…";
    }
}
