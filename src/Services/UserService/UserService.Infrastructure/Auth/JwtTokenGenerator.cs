    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using SynopsisSI.Services.UserService.Application.Interfaces.Infrastructure;
    using SynopsisSI.Services.UserService.Domain.Entities;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    namespace SynopsisSI.Services.UserService.Infrastructure.Auth;

    public class JwtTokenGenerator : ITokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtTokenGenerator> _logger;

        public JwtTokenGenerator(IConfiguration configuration, ILogger<JwtTokenGenerator> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public (string Token, DateTime Expiration) GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32) // Ensure key is strong enough
                throw new InvalidOperationException("JWT SecretKey is not configured or is too short (must be at least 32 bytes).");
            if (string.IsNullOrEmpty(issuer))
                throw new InvalidOperationException("JWT Issuer is not configured.");
            if (string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT Audience is not configured.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature); // Use HmacSha256Signature

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id), // Subject (user ID)
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Username), // Standard claim for username
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token identifier
            };

            // Add roles to claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); // Using System.Security.Claims.ClaimTypes
            }

            var expirationTime = DateTime.UtcNow.AddMinutes(expiryMinutes);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expirationTime,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor); // Use CreateToken
            var token = tokenHandler.WriteToken(securityToken);

            _logger.LogInformation("JWT generated for User ID: {UserId}, Expires: {Expiration}", user.Id, expirationTime);
            return (token, expirationTime);
        }
    }