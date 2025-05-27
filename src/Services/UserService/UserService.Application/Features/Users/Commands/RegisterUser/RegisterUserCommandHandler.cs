        using SynopsisSI.Services.UserService.Application.Interfaces.Persistence;
        using SynopsisSI.Services.UserService.Domain.Entities;
        using SynopsisSI.Shared.Events;
        
        using SynopsisSI.Services.UserService.Application.Interfaces.Infrastructure; 
        using Microsoft.Extensions.Logging;
        using System;
        using System.Threading;
        using System.Threading.Tasks;
        using SynopsisSI.Services.OrderService.Application.Interfaces.MessageBus;

        namespace SynopsisSI.Services.UserService.Application.Features.Users.Commands.RegisterUser;

        public class RegisterUserCommandHandler
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly ILogger<RegisterUserCommandHandler> _logger;
            private readonly IMessageBusPublisher _messageBusPublisher;
            private readonly IPasswordHasher _passwordHasher; 

            public RegisterUserCommandHandler(
                IUnitOfWork unitOfWork,
                ILogger<RegisterUserCommandHandler> logger,
                IMessageBusPublisher messageBusPublisher,
                IPasswordHasher passwordHasher) 
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _messageBusPublisher = messageBusPublisher ?? throw new ArgumentNullException(nameof(messageBusPublisher));
                _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher)); 
            }

            public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Attempting to register user with email {Email} and username {Username}", request.Email, request.Username);

                if (await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken) != null)
                {
                    _logger.LogWarning("Registration failed: Email {Email} already exists.", request.Email);
                    throw new ApplicationException("Email already exists.");
                }
                if (await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken) != null)
                {
                    _logger.LogWarning("Registration failed: Username {Username} already exists.", request.Username);
                    throw new ApplicationException("Username already exists.");
                }

                string hashedPassword = _passwordHasher.HashPassword(request.Password); 

                var user = User.Create(request.Username, request.Email, hashedPassword);
                
                await _unitOfWork.Users.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var userRegisteredEvent = new UserRegisteredEvent
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Timestamp = DateTime.UtcNow
                };
                await _messageBusPublisher.PublishAsync(userRegisteredEvent, cancellationToken);

                _logger.LogInformation("User registered successfully with ID {UserId} and UserRegisteredEvent published.", user.Id);
                return user.Id;
            }
        }