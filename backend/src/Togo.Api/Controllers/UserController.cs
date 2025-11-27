using Microsoft.AspNetCore.Mvc;
using Togo.Api.Models;
using Togo.Domain.Interfaces;

namespace Togo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public UserController(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser([FromHeader(Name = "Authorization")] string? authorization, CancellationToken cancellationToken)
    {
        if (!TryGetToken(authorization, out var token))
        {
            return Unauthorized();
        }

        if (!_tokenService.TryValidateToken(token, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new UserProfileResponse(user.Id, user.Name, user.Email));
    }

    private static bool TryGetToken(string? authorizationHeader, out string token)
    {
        token = string.Empty;
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return false;
        }

        const string bearerPrefix = "Bearer ";
        if (authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            token = authorizationHeader[bearerPrefix.Length..].Trim();
        }
        else
        {
            token = authorizationHeader.Trim();
        }

        return !string.IsNullOrWhiteSpace(token);
    }
}
