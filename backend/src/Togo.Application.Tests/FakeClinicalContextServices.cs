using Togo.Application.Security;

namespace Togo.Application.Tests;

internal sealed class FakeCurrentClinicalContext : ICurrentClinicalContext
{
    public FakeCurrentClinicalContext(long? clinicId = 1) => ClinicId = clinicId;

    public long? ClinicId { get; }

    public bool HasClinic => ClinicId.HasValue;

    public long GetRequiredClinicId() => ClinicId ?? throw new MissingClinicalContextException("A clinical context is required.");
}

internal sealed class FakeClinicalContextAuthorizationService : IClinicalContextAuthorizationService
{
    public bool DenyAccess { get; set; }

    public Task EnsureCanAccessCurrentClinicAsync(CancellationToken cancellationToken = default)
    {
        if (DenyAccess)
        {
            throw new ClinicalContextAccessDeniedException(Guid.NewGuid(), 1);
        }

        return Task.CompletedTask;
    }

    public Task EnsureCanAccessClinicAsync(long clinicId, CancellationToken cancellationToken = default) => EnsureCanAccessCurrentClinicAsync(cancellationToken);
}
