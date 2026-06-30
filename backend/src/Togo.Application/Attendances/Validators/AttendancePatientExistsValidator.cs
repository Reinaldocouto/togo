using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Pets;
using Togo.Application.Tutors;

namespace Togo.Application.Attendances.Validators;

public class AttendancePatientExistsValidator
{
    private readonly IPetRepository _petRepository;
    private readonly ILogger<AttendancePatientExistsValidator> _logger;

    public AttendancePatientExistsValidator(
        IPetRepository petRepository,
        ILogger<AttendancePatientExistsValidator> logger)
    {
        _petRepository = petRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<AttendancePatientScope>> ValidateAsync(long patientId, CancellationToken cancellationToken)
    {
        if (patientId <= 0)
        {
            _logger.LogWarning("Attendance patient existence validation failed because patient id is invalid. PatientId: {PatientId}", patientId);
            return ApplicationResult<AttendancePatientScope>.ValidationError("Patient id is invalid.");
        }

        var patient = await _petRepository.GetByPatientIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            _logger.LogWarning("Attendance patient existence validation failed because patient was not found. PatientId: {PatientId}", patientId);
            return ApplicationResult<AttendancePatientScope>.NotFound("Patient not found.");
        }

        _logger.LogDebug("Attendance patient existence validation succeeded. PatientId: {PatientId}", patientId);
        return ApplicationResult<AttendancePatientScope>.Success(new AttendancePatientScope(patient.PatientId, patient.ClinicId));
    }
}
