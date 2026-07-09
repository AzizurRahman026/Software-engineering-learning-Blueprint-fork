using System.Security.Claims;
using Application.Common.Interfaces.Publisher;
using Application.Features.Auth.Commands.AssignRole;
using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.ResetPassword;
using Application.Features.Auth.Commands.Signup;
using Application.Features.Auth.Commands.UpdateProfile;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Queries.GetUserById;
using Application.Features.Auth.Queries.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protected by default; anonymous endpoints opt out with [AllowAnonymous].
public class AuthController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public AuthController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    // The authenticated user's id, from the JWT 'sub' claim (mapped to NameIdentifier).
    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>Register a new user. Returns an access token + refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("signup")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Signup([FromBody] SignupRequestDto request)
    {
        var response = await _messageBus.SendAsync<SignupCommand, AuthResponseDto>(request.ToSignupCommand());
        return Created(string.Empty, response);
    }

    /// <summary>Authenticate with email or username. Returns an access token + refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var response = await _messageBus.SendAsync<LoginQuery, AuthResponseDto>(request.ToLoginQuery());
        return Ok(response);
    }

    /// <summary>Exchange a valid refresh token for a new access + refresh token (rotation).</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var response = await _messageBus.SendAsync<RefreshTokenCommand, AuthResponseDto>(request.ToRefreshTokenCommand());
        return Ok(response);
    }

    /// <summary>Revoke the current user's refresh token (logout).</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var userId = GetUserId();
        if (userId is not null)
            await _messageBus.SendAsync(new LogoutCommand { UserId = userId });
        return NoContent();
    }

    /// <summary>Request a password reset link by email. Always returns 200 (no account enumeration).</summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var response = await _messageBus.SendAsync<ForgotPasswordCommand, MessageResponseDto>(request.ToForgotPasswordCommand());
        return Ok(response);
    }

    /// <summary>Set a new password using a token from the reset email.</summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponseDto>> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var response = await _messageBus.SendAsync<ResetPasswordCommand, MessageResponseDto>(request.ToResetPasswordCommand());
        return Ok(response);
    }

    /// <summary>Get a user's profile by id.</summary>
    [HttpGet("users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponseDto>> GetProfile(string id)
    {
        var response = await _messageBus.SendAsync<GetUserByIdQuery, AuthResponseDto>(new GetUserByIdQuery { UserId = id });
        return Ok(response);
    }

    /// <summary>Update a user's profile (username, email, password).</summary>
    [HttpPut("users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponseDto>> UpdateProfile(string id, [FromBody] UpdateProfileRequestDto request)
    {
        var response = await _messageBus.SendAsync<UpdateProfileCommand, AuthResponseDto>(request.ToUpdateProfileCommand(id));
        return Ok(response);
    }

    /// <summary>Assign a role (User/Admin) to a user. SuperAdmin only (verified from the JWT).</summary>
    [HttpPut("users/{id}/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponseDto>> AssignRole(string id, [FromBody] AssignRoleRequestDto request)
    {
        var actingUserId = GetUserId();
        if (actingUserId is null)
            return Unauthorized();

        var response = await _messageBus.SendAsync<AssignRoleCommand, AuthResponseDto>(
            request.ToAssignRoleCommand(actingUserId, id));
        return Ok(response);
    }
}
