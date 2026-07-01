namespace Togo.Application.Security;

public interface ICurrentClinicalContext
{
    long? ClinicId { get; }
    bool HasClinic { get; }
    long GetRequiredClinicId();
}
