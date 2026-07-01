namespace Togo.Application.Security;

public sealed class InvalidClinicalContextException : InvalidOperationException
{
    public InvalidClinicalContextException(string message)
        : base(message)
    {
    }
}
