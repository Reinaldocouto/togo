using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Tutors;

namespace Togo.Application.Attendances.Validators;

public class OpenAttendanceValidator
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILogger<OpenAttendanceValidator> _logger;

    public OpenAttendanceValidator(
        IAttendanceRepository attendanceRepository,
        ILogger<OpenAttendanceValidator> logger)
    {
        _attendanceRepository = attendanceRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<bool>> ValidateAsync(long patientId, CancellationToken cancellationToken)
    {
        if (patientId <= 0)
        {
            _logger.LogWarning("Open attendance validation failed because patient id is invalid. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.ValidationError("Patient id is invalid.");
        }

        var hasOpenAttendance = await _attendanceRepository.HasOpenAttendanceForPatientAsync(patientId, cancellationToken);
        if (hasOpenAttendance)
        {
            _logger.LogWarning("Open attendance validation failed because patient already has an open attendance. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.Conflict("Patient already has an open attendance.");
        }

        _logger.LogDebug("Open attendance validation succeeded. PatientId: {PatientId}", patientId);
        return ApplicationResult<bool>.Success(true);
    }
}
