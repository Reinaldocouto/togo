using System.Security.Claims;
using Togo.Application.Security;
using Togo.Domain.Security;

namespace Togo.Api.Security;

public sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private const string SubjectClaimType = "sub";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUserInfo GetCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            throw new CurrentUserResolutionException("An authenticated current user is required.");
        }

        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(SubjectClaimType);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new CurrentUserResolutionException("The authenticated current user identifier is missing or invalid.");
        }

        return new CurrentUserInfo(
            userId,
            principal.FindFirstValue(TogoClaimTypes.Profile),
            IsAuthenticated: true);
    }
}
