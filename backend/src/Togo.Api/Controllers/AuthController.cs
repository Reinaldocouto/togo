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

    public AuthController(IAuthenticateUser authenticateUser, ITokenService tokenService)
    {
        _authenticateUser = authenticateUser;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _authenticateUser.AuthenticateAsync(request.Email, request.Password, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        var token = _tokenService.IssueToken(user);
        var response = new AuthResponse(user.Id, user.Name, user.Email, token);
        return Ok(response);
    }
}
