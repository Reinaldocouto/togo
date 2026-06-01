namespace Togo.Application.Security;

public sealed record CurrentUserInfo(
    Guid UserId,
    string? Profile,
    bool IsAuthenticated);
