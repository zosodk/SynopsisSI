using Microsoft.AspNetCore.Mvc;
using SynopsisSI.Services.UserService.Application.Features.Auth.Commands.LoginUser;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace SynopsisSI.Services.UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginUserCommandHandler _loginUserHandler;
    private readonly ILogger<AuthController> _logger;

    public AuthController(LoginUserCommandHandler loginUserHandler, ILogger<AuthController> logger)
    {
        _loginUserHandler = loginUserHandler ?? throw new ArgumentNullException(nameof(loginUserHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginUserResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginUserResultDto>> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _loginUserHandler.Handle(command, cancellationToken);
            if (!result.IsSuccess) return Unauthorized(new { error = result.Message });
            return Ok(result);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error during login."); return StatusCode(StatusCodes.Status500InternalServerError, "Error during login.");}
    }
}