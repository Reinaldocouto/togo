using System.Text.Json;
using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Auditing;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Attendances.UseCases;

public class CloseAttendanceUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClinicalAuditLogWriter _clinicalAuditLogWriter;
    private readonly ILogger<CloseAttendanceUseCase> _logger;

    public CloseAttendanceUseCase(
        IAttendanceRepository attendanceRepository,
        ICurrentUserService currentUserService,
        IClinicalAuditLogWriter clinicalAuditLogWriter,
        ILogger<CloseAttendanceUseCase> logger)
    {
        _attendanceRepository = attendanceRepository;
        _currentUserService = currentUserService;
        _clinicalAuditLogWriter = clinicalAuditLogWriter;
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

        var currentUser = _currentUserService.GetCurrentUser();

        try
        {
            attendance.Close(request.ClosedAt, currentUser.UserId, DateTime.UtcNow);
            await _attendanceRepository.UpdateAsync(attendance, cancellationToken);
            await WriteClosedAuditLogAsync(attendance, currentUser, cancellationToken);

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

    private async Task WriteClosedAuditLogAsync(Attendance attendance, CurrentUserInfo currentUser, CancellationToken cancellationToken)
    {
        var auditEvent = new ClinicalAuditEvent(
            EntityName: nameof(Attendance),
            EntityId: attendance.Id.ToString(),
            Action: AttendanceAuditActions.Closed,
            UserId: currentUser.UserId,
            UserProfile: currentUser.Profile,
            OccurredAt: DateTime.UtcNow,
            MetadataJson: CreateMetadataJson(attendance.PatientId, attendance.Status));

        await _clinicalAuditLogWriter.WriteAsync(auditEvent, cancellationToken);
    }

    private static string CreateMetadataJson(long patientId, AttendanceStatus status) =>
        JsonSerializer.Serialize(new { PatientId = patientId, Status = status.ToString() });

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
