using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Togo.Api.Models;
using Togo.Domain.Interfaces;

namespace Togo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticateUser _authenticateUser;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticateUser authenticateUser,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _authenticateUser = authenticateUser;
        _tokenService = tokenService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt received for email {Email}", request.Email);

        var user = await _authenticateUser.AuthenticateAsync(request.Email, request.Password, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Login failed for email {Email}: invalid credentials", request.Email);
            return Unauthorized();
        }

        var token = _tokenService.IssueToken(user);
        _logger.LogInformation("Login succeeded for user {UserId} and email {Email}", user.Id, user.Email);

        var response = new AuthResponse(user.Id, user.Name, user.Email, token);
        return Ok(response);
    }
}
