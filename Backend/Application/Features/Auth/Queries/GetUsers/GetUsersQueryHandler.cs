using Application.Common.Interfaces.Repositories;
using Application.Common.Security;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserSummaryDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserSummaryDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Only a SuperAdmin may list users (this is the picker behind role assignment, which is SuperAdmin-only).
        await RoleGuard.EnsureSuperAdminAsync(_userRepository, request.ActingUserId);

        var users = await _userRepository.GetAllAsync<User>();

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            users = users
                .Where(u => u.Username.Contains(search, StringComparison.OrdinalIgnoreCase)
                            || u.Email.Value.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return users
            .OrderBy(u => u.Username)
            .Select(UserSummaryDto.FromEntity)
            .ToList();
    }
}
