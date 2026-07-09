using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Security;
using Application.Common.Security;
using Application.Features.Auth.DTOs;
using Application.Settings;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator tokenGenerator,
        JwtOptions jwtOptions)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _jwtOptions = jwtOptions;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new AuthenticationException("Refresh token is required.");

        var tokenHash = ResetTokenUtil.Hash(request.RefreshToken);
        var user = await _userRepository.GetByRefreshTokenHashAsync(tokenHash);

        // Reject unknown/expired tokens the same way — no distinction leaked.
        if (user is null || !user.IsRefreshTokenValid(tokenHash, DateTime.UtcNow))
            throw new AuthenticationException("Invalid or expired refresh token.");

        // Rotate: Issue stamps a new refresh-token hash on the user, invalidating the one just used.
        var response = AuthTokenIssuer.Issue(_tokenGenerator, _jwtOptions, user);
        await _userRepository.UpdateAsync(user);

        return response;
    }
}
