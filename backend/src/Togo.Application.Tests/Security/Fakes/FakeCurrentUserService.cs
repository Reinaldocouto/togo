using Togo.Application.Security;

namespace Togo.Application.Tests.Security.Fakes;

public sealed class FakeCurrentUserService : ICurrentUserService
{
    public FakeCurrentUserService(Guid userId)
    {
        CurrentUser = new CurrentUserInfo(userId, Profile: null, IsAuthenticated: true);
    }

    public CurrentUserInfo CurrentUser { get; set; }
    public bool ThrowResolutionException { get; set; }

    public CurrentUserInfo GetCurrentUser()
    {
        if (ThrowResolutionException)
        {
            throw new CurrentUserResolutionException("The current user could not be resolved for the test.");
        }

        return CurrentUser;
    }
}
