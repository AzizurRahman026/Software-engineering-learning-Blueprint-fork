using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Security;
using Application.Common.Security;
using Application.Features.Auth.DTOs;
using Application.Settings;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Auth.Queries.Login;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly JwtOptions _jwtOptions;
    private readonly SuperAdminOptions _superAdmin;

    public LoginQueryHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        JwtOptions jwtOptions,
        SuperAdminOptions superAdmin)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _jwtOptions = jwtOptions;
        _superAdmin = superAdmin;
    }

    public async Task<AuthResponseDto> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var identifier = (request.EmailOrUsername ?? string.Empty).Trim();
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrEmpty(password))
            throw new AuthenticationException("Email/username and password are required.");

        var user = await _userRepository.GetByEmailOrUsernameAsync(identifier);
        if (user is null)
            throw new AuthenticationException("Invalid credentials.");

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            throw new AuthenticationException("Invalid credentials.");

        // Idempotent bootstrap: keep the configured super-admin elevated even if promoted after registration.
        if (user.Role != UserRole.SuperAdmin &&
            !string.IsNullOrWhiteSpace(_superAdmin.Email) &&
            string.Equals(_superAdmin.Email.Trim(), user.Email.Value, StringComparison.OrdinalIgnoreCase))
        {
            user.AssignRole(UserRole.SuperAdmin);
        }

        // Mint tokens (stamps a fresh refresh-token hash) and persist that (+ any role change).
        var response = AuthTokenIssuer.Issue(_tokenGenerator, _jwtOptions, user);
        await _userRepository.UpdateAsync(user);

        return response;
    }
}
