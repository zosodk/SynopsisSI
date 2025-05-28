using Microsoft.AspNetCore.Mvc;
using SynopsisSI.Services.UserService.Application.Features.Users.Commands.RegisterUser;
using SynopsisSI.Services.UserService.Application.Features.Users.Queries.GetUserById; // Added
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading;
// using Microsoft.AspNetCore.Authorization; 

namespace SynopsisSI.Services.UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly RegisterUserCommandHandler _registerUserHandler;
    private readonly GetUserByIdQueryHandler _getUserByIdHandler; // Added
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        RegisterUserCommandHandler registerUserHandler,
        GetUserByIdQueryHandler getUserByIdHandler, // Added
        ILogger<UsersController> logger)
    {
        _registerUserHandler = registerUserHandler ?? throw new ArgumentNullException(nameof(registerUserHandler));
        _getUserByIdHandler = getUserByIdHandler ?? throw new ArgumentNullException(nameof(getUserByIdHandler)); // Added
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var userId = await _registerUserHandler.Handle(command, cancellationToken);
            // Return a more meaningful object for the 201 response body.
            // The Location header will point to the GetUserById endpoint.
            return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { userId = userId, message = "User registered successfully." });
        }
        catch (ApplicationException ex) { _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message); return BadRequest(new { error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error during registration."); return StatusCode(StatusCodes.Status500InternalServerError, "Error during registration."); }
    }

    [HttpGet("{id}")]
    // [Authorize] // TODO: Secure this endpoint appropriately
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserViewModel>> GetUserById(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetUserById called with empty or whitespace ID.");
            return BadRequest(new { error = "User ID cannot be empty." });
        }
        try
        {
            var query = new GetUserByIdQuery { Id = id };
            var userViewModel = await _getUserByIdHandler.Handle(query, cancellationToken);

            if (userViewModel == null)
            {
                _logger.LogInformation("User with ID {UserId} not found.", id);
                return NotFound(new { message = $"User with ID {id} not found." });
            }
            _logger.LogInformation("User with ID {UserId} retrieved successfully.", id);
            return Ok(userViewModel);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument error retrieving user {UserId}: {ErrorMessage}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred retrieving user details.");
        }
    }
}