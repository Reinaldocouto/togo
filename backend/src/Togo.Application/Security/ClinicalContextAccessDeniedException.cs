namespace Togo.Application.Security;

public sealed class ClinicalContextAccessDeniedException : UnauthorizedAccessException
{
    public ClinicalContextAccessDeniedException(Guid userId, long clinicId)
        : base($"User '{userId}' does not have active access to clinic '{clinicId}'.")
    {
        UserId = userId;
        ClinicId = clinicId;
    }

    public Guid UserId { get; }

    public long ClinicId { get; }
}
