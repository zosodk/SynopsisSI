using System;
namespace SynopsisSI.Services.UserService.Application.Features.Auth.Commands.LoginUser;
public class LoginUserResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
    public DateTime? TokenExpiration { get; set; }
}