using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Attendances.UseCases;

public class CancelAttendanceUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILogger<CancelAttendanceUseCase> _logger;

    public CancelAttendanceUseCase(
        IAttendanceRepository attendanceRepository,
        ILogger<CancelAttendanceUseCase> logger)
    {
        _attendanceRepository = attendanceRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<AttendanceResponse>> ExecuteAsync(
        long id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Canceling attendance. AttendanceId: {AttendanceId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Attendance cancel failed due to invalid id. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.ValidationError("Attendance id is invalid.");
        }

        var attendance = await _attendanceRepository.GetByIdAsync(id, cancellationToken);
        if (attendance is null)
        {
            _logger.LogWarning("Attendance not found for cancel operation. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.NotFound("Attendance not found.");
        }

        try
        {
            attendance.Cancel();
            await _attendanceRepository.UpdateAsync(attendance, cancellationToken);

            _logger.LogInformation("Attendance canceled successfully. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.Success(ToResponse(attendance));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Attendance cancel failed due to domain state conflict. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.Conflict(ex.Message);
        }
    }

    private static AttendanceResponse ToResponse(Attendance attendance) =>
        new(
            attendance.Id,
            attendance.PatientId,
            attendance.AttendanceNumber,
            attendance.OpenedAt,
            attendance.ClosedAt,
            attendance.Status,
            attendance.Type);
}
