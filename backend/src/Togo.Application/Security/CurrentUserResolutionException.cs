namespace Togo.Application.Security;

public sealed class CurrentUserResolutionException : InvalidOperationException
{
    public CurrentUserResolutionException(string message)
        : base(message)
    {
    }
}
