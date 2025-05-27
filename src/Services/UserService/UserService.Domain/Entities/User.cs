using System;
using System.Collections.Generic;
using SynopsisSI.Services.UserService.Domain.ValueObjects;

namespace SynopsisSI.Services.UserService.Domain.Entities;

public class User
{
    public string Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public AddressVO? PrimaryAddress { get; private set; }
    public List<string> Roles { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private User()
    {
        Id = Guid.NewGuid().ToString(); 
        Username = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        Roles = new List<string>();
        IsActive = true;
    }

    public static User Create(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        var user = new User
        {
            Username = username.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = new List<string> { "User" }
        };
        return user;
    }

    public void UpdateProfile(string? newUsername, string? newProfileImageUrl, AddressVO? newAddress)
    {
        if (!string.IsNullOrWhiteSpace(newUsername)) Username = newUsername.Trim();
        ProfileImageUrl = newProfileImageUrl;
        PrimaryAddress = newAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("New password hash is required.", nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role) || Roles.Contains(role, StringComparer.OrdinalIgnoreCase)) return;
        Roles.Add(role);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return;
        Roles.RemoveAll(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        UpdatedAt = DateTime.UtcNow;
    }


    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
}