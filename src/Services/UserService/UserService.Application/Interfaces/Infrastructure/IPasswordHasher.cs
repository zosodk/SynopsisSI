namespace SynopsisSI.Services.UserService.Application.Interfaces.Infrastructure;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}