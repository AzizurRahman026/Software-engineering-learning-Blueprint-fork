using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserRepository _userRepository;

    public LogoutCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return;

        var user = await _userRepository.GetByIdAsync<User>(request.UserId);
        if (user is null)
            return; // idempotent — nothing to revoke

        user.ClearRefreshToken();
        await _userRepository.UpdateAsync(user);
    }
}
