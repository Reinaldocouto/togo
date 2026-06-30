using System.Text.Json;
using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Attendances.Validators;
using Togo.Application.Auditing;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Attendances.UseCases;

public class CreateAttendanceUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly AttendancePatientExistsValidator _attendancePatientExistsValidator;
    private readonly AttendanceNumberUniqueValidator _attendanceNumberUniqueValidator;
    private readonly OpenAttendanceValidator _openAttendanceValidator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClinicalAuditLogWriter _clinicalAuditLogWriter;
    private readonly ILogger<CreateAttendanceUseCase> _logger;

    public CreateAttendanceUseCase(
        IAttendanceRepository attendanceRepository,
        AttendancePatientExistsValidator attendancePatientExistsValidator,
        AttendanceNumberUniqueValidator attendanceNumberUniqueValidator,
        OpenAttendanceValidator openAttendanceValidator,
        ICurrentUserService currentUserService,
        IClinicalAuditLogWriter clinicalAuditLogWriter,
        ILogger<CreateAttendanceUseCase> logger)
    {
        _attendanceRepository = attendanceRepository;
        _attendancePatientExistsValidator = attendancePatientExistsValidator;
        _attendanceNumberUniqueValidator = attendanceNumberUniqueValidator;
        _openAttendanceValidator = openAttendanceValidator;
        _currentUserService = currentUserService;
        _clinicalAuditLogWriter = clinicalAuditLogWriter;
        _logger = logger;
    }

    public async Task<ApplicationResult<AttendanceResponse>> ExecuteAsync(
        CreateAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating attendance. PatientId: {PatientId}", request.PatientId);

        var patientValidation = await _attendancePatientExistsValidator.ValidateAsync(request.PatientId, cancellationToken);
        if (!patientValidation.IsSuccess)
        {
            _logger.LogWarning("Attendance creation failed because patient validation did not succeed. PatientId: {PatientId}", request.PatientId);
            return ToAttendanceResponseResult(patientValidation);
        }

        var attendanceNumberValidation = await _attendanceNumberUniqueValidator.ValidateAsync(request.AttendanceNumber, cancellationToken);
        if (!attendanceNumberValidation.IsSuccess)
        {
            _logger.LogWarning("Attendance creation failed because attendance number validation did not succeed. PatientId: {PatientId}", request.PatientId);
            return ToAttendanceResponseResult(attendanceNumberValidation);
        }

        var openAttendanceValidation = await _openAttendanceValidator.ValidateAsync(request.PatientId, cancellationToken);
        if (!openAttendanceValidation.IsSuccess)
        {
            _logger.LogWarning("Attendance creation failed because patient already has open attendance. PatientId: {PatientId}", request.PatientId);
            return ToAttendanceResponseResult(openAttendanceValidation);
        }

        try
        {
            var currentUser = _currentUserService.GetCurrentUser();
            var createdAtUtc = DateTime.UtcNow;
            var patientScope = patientValidation.Data!;
            var attendance = Attendance.Create(patientScope.ClinicId, request.PatientId, request.AttendanceNumber, request.OpenedAt, request.Type, currentUser.UserId, createdAtUtc);
            await _attendanceRepository.AddAsync(attendance, cancellationToken);
            await WriteCreatedAuditLogAsync(attendance, currentUser, cancellationToken);

            _logger.LogInformation("Attendance created successfully. PatientId: {PatientId}. AttendanceId: {AttendanceId}", request.PatientId, attendance.Id);
            return ApplicationResult<AttendanceResponse>.Success(ToResponse(attendance));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Attendance creation failed due to domain validation error");
            return ApplicationResult<AttendanceResponse>.ValidationError(ex.Message);
        }
    }

    private async Task WriteCreatedAuditLogAsync(Attendance attendance, CurrentUserInfo currentUser, CancellationToken cancellationToken)
    {
        var auditEvent = new ClinicalAuditEvent(
            EntityName: nameof(Attendance),
            EntityId: attendance.Id.ToString(),
            Action: AttendanceAuditActions.Created,
            UserId: currentUser.UserId,
            UserProfile: currentUser.Profile,
            OccurredAt: DateTime.UtcNow,
            MetadataJson: CreateMetadataJson(attendance.ClinicId, attendance.PatientId, attendance.Status));

        await _clinicalAuditLogWriter.WriteAsync(auditEvent, cancellationToken);
    }

    private static string CreateMetadataJson(long clinicId, long patientId, AttendanceStatus status) =>
        JsonSerializer.Serialize(new { ClinicId = clinicId, PatientId = patientId, Status = status.ToString() });

    private static ApplicationResult<AttendanceResponse> ToAttendanceResponseResult<T>(ApplicationResult<T> validationResult) =>
        validationResult.Type switch
        {
            ApplicationResultType.NotFound => ApplicationResult<AttendanceResponse>.NotFound(validationResult.Error!),
            ApplicationResultType.ValidationError => ApplicationResult<AttendanceResponse>.ValidationError(validationResult.Error!),
            ApplicationResultType.Conflict => ApplicationResult<AttendanceResponse>.Conflict(validationResult.Error!),
            _ => ApplicationResult<AttendanceResponse>.Failure(validationResult.Error ?? "Attendance validation failed.")
        };

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
