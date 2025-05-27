using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SynopsisSI.Services.UserService.Application.Interfaces.Infrastructure;
using System;
using System.Security.Cryptography;

    namespace SynopsisSI.Services.UserService.Infrastructure.Security;

    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 128 / 8; // 16 bytes
        private const int KeySize = 256 / 8;  // 32 bytes
        private const int Iterations = 10000; // Number of iterations for PBKDF2
        private static readonly KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            // Generate a random salt
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Hash the password
            byte[] subkey = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: Prf,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            // Combine salt and subkey
            var outputBytes = new byte[SaltSize + KeySize];
            Buffer.BlockCopy(salt, 0, outputBytes, 0, SaltSize);
            Buffer.BlockCopy(subkey, 0, outputBytes, SaltSize, KeySize);

            return Convert.ToBase64String(outputBytes);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
                return false;

            byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            // Ensure the hashed password has the correct length (salt + key)
            if (hashedPasswordBytes.Length != SaltSize + KeySize)
                return false; // Invalid format

            // Extract salt from the beginning of the hashed password
            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 0, salt, 0, SaltSize);

            // Extract the subkey (the actual hash) from the rest of the hashed password
            byte[] expectedSubkey = new byte[KeySize];
            Buffer.BlockCopy(hashedPasswordBytes, SaltSize, expectedSubkey, 0, KeySize);

            // Hash the provided password with the extracted salt
            byte[] actualSubkey = KeyDerivation.Pbkdf2(
                password: providedPassword,
                salt: salt,
                prf: Prf,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            // Compare the hashes in a way that protects against timing attacks
            return CryptographicOperations.FixedTimeEquals(expectedSubkey, actualSubkey);
        }
    }
