using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynopsisSI.Services.UserService.Infrastructure.Security;
using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;


namespace SynopsisSI.Tests.UserService.Infrastructure;

[TestClass]
public class PasswordHasherTests
{
    private PasswordHasher _passwordHasher;

    [TestInitialize]
    public void Setup()
    {
        _passwordHasher = new PasswordHasher();
    }

    [TestMethod]
    public void HashPassword_WithValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        string hash = _passwordHasher.HashPassword(password);

        // Assert
        Assert.IsNotNull(hash);
        Assert.IsTrue(hash.Length > 0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void HashPassword_WithNullPassword_ThrowsArgumentNullException()
    {
        // Act
        _passwordHasher.HashPassword(null);
    }

    [TestMethod]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        string password = "TestPassword123!";
        string hash = _passwordHasher.HashPassword(password);

        // Act
        bool result = _passwordHasher.VerifyPassword(hash, password);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        string wrongPassword = "WrongPassword123!";
        string hash = _passwordHasher.HashPassword(password);

        // Act
        bool result = _passwordHasher.VerifyPassword(hash, wrongPassword);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void VerifyPassword_WithInvalidHash_ReturnsFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        string invalidHash = "InvalidHash";

        // Act
        bool result = _passwordHasher.VerifyPassword(invalidHash, password);

        // Assert
        Assert.IsFalse(result);
    }
}