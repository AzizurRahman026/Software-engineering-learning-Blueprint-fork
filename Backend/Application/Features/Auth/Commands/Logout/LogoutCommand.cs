using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest
{
    // The authenticated user (from the access token's sub claim).
    public string UserId { get; set; } = string.Empty;
}
