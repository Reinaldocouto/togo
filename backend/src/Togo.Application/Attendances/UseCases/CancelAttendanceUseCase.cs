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

public class CancelAttendanceUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClinicalAuditLogWriter _clinicalAuditLogWriter;
    private readonly ILogger<CancelAttendanceUseCase> _logger;

    public CancelAttendanceUseCase(
        IAttendanceRepository attendanceRepository,
        ICurrentUserService currentUserService,
        IClinicalAuditLogWriter clinicalAuditLogWriter,
        ILogger<CancelAttendanceUseCase> logger)
    {
        _attendanceRepository = attendanceRepository;
        _currentUserService = currentUserService;
        _clinicalAuditLogWriter = clinicalAuditLogWriter;
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

        var currentUser = _currentUserService.GetCurrentUser();

        try
        {
            attendance.Cancel(currentUser.UserId, DateTime.UtcNow);
            await _attendanceRepository.UpdateAsync(attendance, cancellationToken);
            await WriteCanceledAuditLogAsync(attendance, currentUser, cancellationToken);

            _logger.LogInformation("Attendance canceled successfully. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.Success(ToResponse(attendance));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Attendance cancel failed due to domain state conflict. AttendanceId: {AttendanceId}", id);
            return ApplicationResult<AttendanceResponse>.Conflict(ex.Message);
        }
    }

    private async Task WriteCanceledAuditLogAsync(Attendance attendance, CurrentUserInfo currentUser, CancellationToken cancellationToken)
    {
        var auditEvent = new ClinicalAuditEvent(
            EntityName: nameof(Attendance),
            EntityId: attendance.Id.ToString(),
            Action: AttendanceAuditActions.Canceled,
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
            attendance.ClinicId,
            attendance.PatientId,
            attendance.AttendanceNumber,
            attendance.OpenedAt,
            attendance.ClosedAt,
            attendance.Status,
            attendance.Type);
}
