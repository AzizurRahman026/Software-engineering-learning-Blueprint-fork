using Application.Common.Interfaces.Repositories;
using Application.Common.Security;
using Application.Common.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Posts.Commands.RejectPost;

public class RejectPostCommandHandler : IRequestHandler<RejectPostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cache;

    public RejectPostCommandHandler(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ICacheService cache)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task Handle(RejectPostCommand request, CancellationToken cancellationToken)
    {
        await RoleGuard.EnsureAdminAsync(_userRepository, request.ActingUserId);

        var post = await _postRepository.GetByIdAsync<Post>(request.PostId)
            ?? throw new NotFoundException("Post not found.");

        post.Status = PostStatus.Rejected;
        await _postRepository.UpdateAsync(post);

        await _cache.RemoveAsync(PostCacheKeys.Detail(post.Id), cancellationToken);
    }
}
