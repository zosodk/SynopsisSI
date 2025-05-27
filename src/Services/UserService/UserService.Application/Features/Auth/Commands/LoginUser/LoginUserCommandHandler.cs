        using SynopsisSI.Services.UserService.Application.Interfaces.Persistence;
        using SynopsisSI.Services.UserService.Application.Interfaces.Infrastructure;
        using Microsoft.Extensions.Logging;
        using System;
        using System.Threading;
        using System.Threading.Tasks;

        namespace SynopsisSI.Services.UserService.Application.Features.Auth.Commands.LoginUser;

        public class LoginUserCommandHandler
        {
            private readonly IUserRepository _userRepository;
            private readonly ITokenGenerator _tokenGenerator;
            private readonly IPasswordHasher _passwordHasher; 
            private readonly ILogger<LoginUserCommandHandler> _logger;

            public LoginUserCommandHandler(
                IUserRepository userRepository,
                ITokenGenerator tokenGenerator,
                IPasswordHasher passwordHasher, 
                ILogger<LoginUserCommandHandler> logger)
            {
                _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
                _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
                _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher)); 
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<LoginUserResultDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);
                var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                    return new LoginUserResultDto { IsSuccess = false, Message = "Invalid email or password." };
                }

                bool isPasswordValid = _passwordHasher.VerifyPassword(user.PasswordHash, request.Password); 

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);
                    return new LoginUserResultDto { IsSuccess = false, Message = "Invalid email or password." };
                }
                
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User account {Email} is inactive.", request.Email);
                    return new LoginUserResultDto { IsSuccess = false, Message = "Account is inactive." };
                }

                var (token, expiration) = _tokenGenerator.GenerateToken(user);
                _logger.LogInformation("Login successful for user {Email}. Token generated.", request.Email);

                return new LoginUserResultDto
                {
                    IsSuccess = true,
                    Message = "Login successful",
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Token = token,
                    TokenExpiration = expiration
                };
            }
        }
