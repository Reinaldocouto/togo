namespace Togo.Application.Security;

public interface ICurrentUserService
{
    CurrentUserInfo GetCurrentUser();
}
