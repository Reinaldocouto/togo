using Microsoft.Extensions.Logging;
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

    public async Task<ApplicationResult<bool>> ValidateAsync(long patientId, CancellationToken cancellationToken)
    {
        if (patientId <= 0)
        {
            _logger.LogWarning("Attendance patient existence validation failed because patient id is invalid. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.ValidationError("Patient id is invalid.");
        }

        var patient = await _petRepository.GetByPatientIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            _logger.LogWarning("Attendance patient existence validation failed because patient was not found. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.NotFound("Patient not found.");
        }

        _logger.LogDebug("Attendance patient existence validation succeeded. PatientId: {PatientId}", patientId);
        return ApplicationResult<bool>.Success(true);
    }
}
