using Microsoft.AspNetCore.Mvc;
using SynopsisSI.Services.UserService.Application.Features.Users.Commands.RegisterUser;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace SynopsisSI.Services.UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly RegisterUserCommandHandler _registerUserHandler;
    private readonly ILogger<UsersController> _logger;

    public UsersController(RegisterUserCommandHandler registerUserHandler, ILogger<UsersController> logger)
    {
        _registerUserHandler = registerUserHandler ?? throw new ArgumentNullException(nameof(registerUserHandler));
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
            return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { id = userId });
        }
        catch (ApplicationException ex) { _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message); return BadRequest(new { error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error during registration."); return StatusCode(StatusCodes.Status500InternalServerError, "Error during registration."); }
    }

    [HttpGet("{id}")]
    // [Authorize] // TODO: Secure this
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUserById(string id)
    {
        _logger.LogInformation("GetUserById called for ID: {UserId} (Not fully implemented)", id);
        return Ok(new { UserId = id, Message = "GetUserById - Not fully implemented." });
    }
}