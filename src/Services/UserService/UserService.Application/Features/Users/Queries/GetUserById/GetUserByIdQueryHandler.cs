using SynopsisSI.Services.UserService.Application.Interfaces.Persistence;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
// using AutoMapper; // If you were using AutoMapper

namespace SynopsisSI.Services.UserService.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;
    // private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IUserRepository userRepository, ILogger<GetUserByIdQueryHandler> logger /*, IMapper mapper*/)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // _mapper = mapper;
    }

    public async Task<UserViewModel?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Id))
        {
            _logger.LogWarning("GetUserByIdQuery handled with invalid request (null or empty ID).");
            throw new ArgumentException("User ID must be provided.", nameof(request.Id));
        }

        _logger.LogInformation("Fetching user with ID: {UserId}", request.Id);
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            _logger.LogInformation("User with ID {UserId} not found.", request.Id);
            return null;
        }

        // Manual mapping
        var userViewModel = new UserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            ProfileImageUrl = user.ProfileImageUrl,
            PrimaryAddress = user.PrimaryAddress != null ? new AddressViewModel
            {
                Street = user.PrimaryAddress.Street,
                City = user.PrimaryAddress.City,
                PostalCode = user.PrimaryAddress.PostalCode,
                Country = user.PrimaryAddress.Country
            } : null,
            Roles = user.Roles,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
        // var userViewModel = _mapper.Map<UserViewModel>(user); // If using AutoMapper

        _logger.LogInformation("User with ID {UserId} retrieved successfully.", request.Id);
        return userViewModel;
    }
}