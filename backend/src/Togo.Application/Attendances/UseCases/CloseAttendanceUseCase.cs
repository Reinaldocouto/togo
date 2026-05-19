using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Attendances.UseCases;

public class CloseAttendanceUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILogger<CloseAttendanceUseCase> _logger;

    public CloseAttendanceUseCase(
        IAttendanceRepository attendanceRepository,
        ILogger<CloseAttendanceUseCase> logger)
    {
        _attendanceRepository = attendanceRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<AttendanceResponse>> ExecuteAsync(
        long id,
        CloseAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Closing attendance. AttendanceId: {AttendanceId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Attendance close failed due to invalid id. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.ValidationError("Attendance id is invalid.");
        }

        var attendance = await _attendanceRepository.GetByIdAsync(id, cancellationToken);
        if (attendance is null)
        {
            _logger.LogWarning("Attendance not found for close operation. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.NotFound("Attendance not found.");
        }

        try
        {
            attendance.Close(request.ClosedAt);
            await _attendanceRepository.UpdateAsync(attendance, cancellationToken);

            _logger.LogInformation("Attendance closed successfully. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.Success(ToResponse(attendance));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Attendance close failed due to domain validation error. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.ValidationError(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Attendance close failed due to domain state conflict. AttendanceId: {AttendanceId}", id);
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
