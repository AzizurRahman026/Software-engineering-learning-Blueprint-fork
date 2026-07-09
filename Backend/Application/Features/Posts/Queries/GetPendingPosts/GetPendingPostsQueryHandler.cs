using Application.Common.Interfaces.Repositories;
using Application.Common.Security;
using Application.Features.Posts.DTOs;
using MediatR;

namespace Application.Features.Posts.Queries.GetPendingPosts;

public class GetPendingPostsQueryHandler : IRequestHandler<GetPendingPostsQuery, List<PostSummaryDto>>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;

    public GetPendingPostsQueryHandler(IPostRepository postRepository, IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
    }

    public async Task<List<PostSummaryDto>> Handle(GetPendingPostsQuery request, CancellationToken cancellationToken)
    {
        await RoleGuard.EnsureAdminAsync(_userRepository, request.ActingUserId);

        var pending = await _postRepository.GetPendingAsync();
        return pending.Select(PostSummaryDto.FromEntity).ToList();
    }
}
