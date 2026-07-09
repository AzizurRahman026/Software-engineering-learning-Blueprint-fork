using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Security;
using Application.Settings;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Posts.Commands.PublishPost;

public class PublishPostCommandHandler : IRequestHandler<PublishPostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly IEmailSender _emailSender;
    private readonly ICacheService _cache;
    private readonly PasswordResetOptions _options; // reused for its FrontendUrl (frontend base URL)
    private readonly ILogger<PublishPostCommandHandler> _logger;

    public PublishPostCommandHandler(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ISubscriberRepository subscriberRepository,
        IEmailSender emailSender,
        ICacheService cache,
        PasswordResetOptions options,
        ILogger<PublishPostCommandHandler> logger)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _subscriberRepository = subscriberRepository;
        _emailSender = emailSender;
        _cache = cache;
        _options = options;
        _logger = logger;
    }

    public async Task Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        await RoleGuard.EnsureAdminAsync(_userRepository, request.ActingUserId);

        var post = await _postRepository.GetByIdAsync<Post>(request.PostId)
            ?? throw new NotFoundException("Post not found.");

        // Idempotent: republishing an already-published post is a no-op (no duplicate newsletter).
        if (post.Status == PostStatus.Published)
            return;

        post.Status = PostStatus.Published;
        post.PublishedAt = DateTime.UtcNow;
        await _postRepository.UpdateAsync(post);

        // Evict any stale gated snapshot so the now-public post is served fresh.
        await _cache.RemoveAsync(PostCacheKeys.Detail(post.Id), cancellationToken);

        await FanOutToSubscribersAsync(post, cancellationToken);
    }

    // Emails the newly published post to every confirmed subscriber. One failure never aborts the batch.
    private async Task FanOutToSubscribersAsync(Post post, CancellationToken cancellationToken)
    {
        var subscribers = await _subscriberRepository.GetConfirmedAsync();
        if (subscribers.Count == 0)
            return;

        var frontend = _options.FrontendUrl.TrimEnd('/');
        var postLink = $"{frontend}/posts/{post.Id}";

        foreach (var subscriber in subscribers)
        {
            try
            {
                var unsubscribeLink = $"{frontend}/unsubscribe?token={subscriber.UnsubscribeToken}";
                await _emailSender.SendAsync(
                    subscriber.Email.Value,
                    post.Title,
                    BuildEmailBody(post, postLink, unsubscribeLink),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send newsletter for post {PostId} to a subscriber.", post.Id);
            }
        }

        _logger.LogInformation("Newsletter fan-out complete for post {PostId} to {Count} subscribers.",
            post.Id, subscribers.Count);
    }

    private static string BuildEmailBody(Post post, string postLink, string unsubscribeLink) => $@"
        <div style=""font-family:Arial,sans-serif;font-size:15px;color:#1a1a1a;line-height:1.6"">
            <h2 style=""margin:0 0 12px"">{System.Net.WebUtility.HtmlEncode(post.Title)}</h2>
            <p>{System.Net.WebUtility.HtmlEncode(post.Summary)}</p>
            <p style=""margin:24px 0"">
                <a href=""{postLink}""
                   style=""background:#2563eb;color:#fff;padding:12px 22px;border-radius:8px;text-decoration:none;display:inline-block"">
                    Read the full post
                </a>
            </p>
            <hr style=""border:none;border-top:1px solid #eee;margin:24px 0"">
            <p style=""font-size:12px;color:#999"">
                You're receiving this because you subscribed to our newsletter.
                <a href=""{unsubscribeLink}"" style=""color:#999"">Unsubscribe</a>.
            </p>
        </div>";
}
