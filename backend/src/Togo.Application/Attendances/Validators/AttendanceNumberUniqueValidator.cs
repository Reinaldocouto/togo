using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Tutors;

namespace Togo.Application.Attendances.Validators;

public class AttendanceNumberUniqueValidator
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILogger<AttendanceNumberUniqueValidator> _logger;

    public AttendanceNumberUniqueValidator(
        IAttendanceRepository attendanceRepository,
        ILogger<AttendanceNumberUniqueValidator> logger)
    {
        _attendanceRepository = attendanceRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<bool>> ValidateAsync(string? attendanceNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(attendanceNumber))
        {
            _logger.LogWarning("Attendance number uniqueness validation failed because attendance number was not provided");
            return ApplicationResult<bool>.ValidationError("Attendance number is required.");
        }

        var normalizedAttendanceNumber = attendanceNumber.Trim();
        var exists = await _attendanceRepository.ExistsByAttendanceNumberAsync(normalizedAttendanceNumber, cancellationToken);
        if (exists)
        {
            _logger.LogWarning("Attendance number uniqueness validation failed. HasAttendanceNumber: {HasAttendanceNumber}", true);
            return ApplicationResult<bool>.Conflict("An attendance with this number already exists.");
        }

        _logger.LogDebug("Attendance number uniqueness validation succeeded. HasAttendanceNumber: {HasAttendanceNumber}", true);
        return ApplicationResult<bool>.Success(true);
    }
}
