using Application.Features.Auth.Commands.ResetPassword;

namespace Application.Features.Auth.DTOs;

public class ResetPasswordRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;

    public ResetPasswordCommand ToResetPasswordCommand() => new()
    {
        Token = Token,
        NewPassword = NewPassword
    };
}
