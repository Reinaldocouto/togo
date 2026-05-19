using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Attendances.UseCases;

public class GetAttendanceByIdUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILogger<GetAttendanceByIdUseCase> _logger;

    public GetAttendanceByIdUseCase(
        IAttendanceRepository attendanceRepository,
        ILogger<GetAttendanceByIdUseCase> logger)
    {
        _attendanceRepository = attendanceRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<AttendanceResponse>> ExecuteAsync(
        long id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting attendance by id. AttendanceId: {AttendanceId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Attendance get by id failed due to invalid id. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.ValidationError("Attendance id is invalid.");
        }

        var attendance = await _attendanceRepository.GetByIdAsync(id, cancellationToken);
        if (attendance is null)
        {
            _logger.LogWarning("Attendance not found. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.NotFound("Attendance not found.");
        }

        var response = ToResponse(attendance);

        _logger.LogInformation("Attendance found. AttendanceId: {AttendanceId}", response.Id);

        return ApplicationResult<AttendanceResponse>.Success(response);
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
