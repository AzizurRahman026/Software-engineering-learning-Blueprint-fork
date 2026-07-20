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
using Application.Features.Auth.Queries.GetUsers;
using Application.Features.Auth.Queries.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protected by default; anonymous endpoints opt out with [AllowAnonymous].
public class AuthController : ApiControllerBase
{
    private readonly IMessageBus _messageBus;

    public AuthController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    /// <summary>Register a new user. Returns an access token + refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("signup")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Signup([FromBody] SignupRequestDto request)
    {
        var signupCommand = request.ToSignupCommand();
        var response = await _messageBus.SendAsync<SignupCommand, AuthResponseDto>(signupCommand);
        return Created(string.Empty, response);
    }

    /// <summary>Authenticate with email or username. Returns an access token + refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var loginQuery = request.ToLoginQuery();
      var response = await _messageBus.SendAsync<LoginQuery, AuthResponseDto>(loginQuery);
        return Ok(response);
    }

    /// <summary>Exchange a valid refresh token for a new access + refresh token (rotation).</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var refreshTokenCommand = request.ToRefreshTokenCommand();
        var response = await _messageBus.SendAsync<RefreshTokenCommand, AuthResponseDto>(refreshTokenCommand);
        return Ok(response);
    }

    /// <summary>Revoke the current user's refresh token (logout).</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var userId = GetUserId();
        var logoutCommand = new LogoutCommand() { UserId = userId! };
        if (userId is not null)
            await _messageBus.SendAsync(logoutCommand);
        return NoContent();
    }

    /// <summary>Request a password reset link by email. Always returns 200 (no account enumeration).</summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var forgotPasswordCommand = request.ToForgotPasswordCommand();
        var response = await _messageBus.SendAsync<ForgotPasswordCommand, MessageResponseDto>(forgotPasswordCommand);
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

    /// <summary>List users for role management. SuperAdmin only. Optional case-insensitive search over username/email.</summary>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<UserSummaryDto>>> GetUsers([FromQuery] string? search)
    {
        var actingUserId = GetUserId();
        if (actingUserId is null)
            return Unauthorized();

        var users = await _messageBus.SendAsync<GetUsersQuery, List<UserSummaryDto>>(
            new GetUsersQuery { ActingUserId = actingUserId, Search = search });
        return Ok(users);
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
