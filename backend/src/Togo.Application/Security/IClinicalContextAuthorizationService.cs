namespace Togo.Application.Security;

public interface IClinicalContextAuthorizationService
{
    Task EnsureCanAccessCurrentClinicAsync(CancellationToken cancellationToken = default);

    Task EnsureCanAccessClinicAsync(long clinicId, CancellationToken cancellationToken = default);
}
