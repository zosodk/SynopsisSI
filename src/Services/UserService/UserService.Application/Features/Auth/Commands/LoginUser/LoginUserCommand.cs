using System.ComponentModel.DataAnnotations;
namespace SynopsisSI.Services.UserService.Application.Features.Auth.Commands.LoginUser;
public class LoginUserCommand
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}