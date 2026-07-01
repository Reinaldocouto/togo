namespace Togo.Application.Security;

public sealed class MissingClinicalContextException : InvalidOperationException
{
    public MissingClinicalContextException()
        : base("A clinical context is required for this operation.")
    {
    }
}
