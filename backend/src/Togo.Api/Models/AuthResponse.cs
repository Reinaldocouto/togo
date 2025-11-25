namespace Togo.Api.Models;

public record AuthResponse(Guid UserId, string Name, string Email, string Token);
