    using Microsoft.AspNetCore.Cryptography.KeyDerivation;
    using SynopsisSI.Services.UserService.Application.Interfaces.Infrastructure;
    using System;
    using System.Security.Cryptography;

    namespace SynopsisSI.Services.UserService.Infrastructure.Security;

    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 128 / 8; // 16 bytes for a 128-bit salt
        private const int KeySize = 256 / 8;  // 32 bytes for a 256-bit subkey (HMACSHA256 output)
        private const int Iterations = 100000; // NIST recommendation for PBKDF2 is at least 10,000. Higher is better.
        private static readonly KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256; // Use HMACSHA256

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] subkey = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: Prf,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            var outputBytes = new byte[SaltSize + KeySize];
            Buffer.BlockCopy(salt, 0, outputBytes, 0, SaltSize);           // Prepend salt
            Buffer.BlockCopy(subkey, 0, outputBytes, SaltSize, KeySize);  // Append hashed password

            return Convert.ToBase64String(outputBytes);
        }

        public bool VerifyPassword(string hashedPasswordWithSalt, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPasswordWithSalt) || string.IsNullOrEmpty(providedPassword))
                return false;

            byte[] hashedPasswordBytes;
            try
            {
                hashedPasswordBytes = Convert.FromBase64String(hashedPasswordWithSalt);
            }
            catch (FormatException)
            {
                // Invalid Base64 string
                return false;
            }

            if (hashedPasswordBytes.Length != SaltSize + KeySize)
                return false; // Stored hash is not the correct length

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 0, salt, 0, SaltSize);

            byte[] expectedSubkey = new byte[KeySize];
            Buffer.BlockCopy(hashedPasswordBytes, SaltSize, expectedSubkey, 0, KeySize);

            byte[] actualSubkey = KeyDerivation.Pbkdf2(
                password: providedPassword,
                salt: salt,
                prf: Prf,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            return CryptographicOperations.FixedTimeEquals(expectedSubkey, actualSubkey);
        }
    }