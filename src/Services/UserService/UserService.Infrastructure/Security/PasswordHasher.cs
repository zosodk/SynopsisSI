using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SynopsisSI.Services.UserService.Application.Interfaces.Infrastructure;
using System;
using System.Security.Cryptography;

namespace SynopsisSI.Services.UserService.Infrastructure.Security;
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8; // 16 bytes
    private const int KeySize = 256 / 8;  // 32 bytes
    private const int Iterations = 100000;
    private static readonly KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] subkey = KeyDerivation.Pbkdf2(password, salt, Prf, Iterations, KeySize);
        var outputBytes = new byte[SaltSize + KeySize];
        Buffer.BlockCopy(salt, 0, outputBytes, 0, SaltSize);
        Buffer.BlockCopy(subkey, 0, outputBytes, SaltSize, KeySize);
        return Convert.ToBase64String(outputBytes);
    }

    public bool VerifyPassword(string hashedPasswordWithSalt, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPasswordWithSalt) || string.IsNullOrEmpty(providedPassword)) return false;
        byte[] hashedPasswordBytes;
        try { hashedPasswordBytes = Convert.FromBase64String(hashedPasswordWithSalt); }
        catch (FormatException) { return false; }
        if (hashedPasswordBytes.Length != SaltSize + KeySize) return false;
        byte[] salt = new byte[SaltSize];
        Buffer.BlockCopy(hashedPasswordBytes, 0, salt, 0, SaltSize);
        byte[] expectedSubkey = new byte[KeySize];
        Buffer.BlockCopy(hashedPasswordBytes, SaltSize, expectedSubkey, 0, KeySize);
        byte[] actualSubkey = KeyDerivation.Pbkdf2(providedPassword, salt, Prf, Iterations, KeySize);
        return CryptographicOperations.FixedTimeEquals(expectedSubkey, actualSubkey);
    }
}