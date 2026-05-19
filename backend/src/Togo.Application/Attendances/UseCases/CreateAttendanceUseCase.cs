using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Attendances.Validators;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Attendances.UseCases;

public class CreateAttendanceUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly AttendancePatientExistsValidator _attendancePatientExistsValidator;
    private readonly AttendanceNumberUniqueValidator _attendanceNumberUniqueValidator;
    private readonly OpenAttendanceValidator _openAttendanceValidator;
    private readonly ILogger<CreateAttendanceUseCase> _logger;

    public CreateAttendanceUseCase(
        IAttendanceRepository attendanceRepository,
        AttendancePatientExistsValidator attendancePatientExistsValidator,
        AttendanceNumberUniqueValidator attendanceNumberUniqueValidator,
        OpenAttendanceValidator openAttendanceValidator,
        ILogger<CreateAttendanceUseCase> logger)
    {
        _attendanceRepository = attendanceRepository;
        _attendancePatientExistsValidator = attendancePatientExistsValidator;
        _attendanceNumberUniqueValidator = attendanceNumberUniqueValidator;
        _openAttendanceValidator = openAttendanceValidator;
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
            var attendance = Attendance.Create(request.PatientId, request.AttendanceNumber, request.OpenedAt, request.Type);
            await _attendanceRepository.AddAsync(attendance, cancellationToken);

            _logger.LogInformation("Attendance created successfully. PatientId: {PatientId}. AttendanceId: {AttendanceId}", request.PatientId, attendance.Id);
            return ApplicationResult<AttendanceResponse>.Success(ToResponse(attendance));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Attendance creation failed due to domain validation error");
            return ApplicationResult<AttendanceResponse>.ValidationError(ex.Message);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Attendance creation failed due to domain validation error");
            return ApplicationResult<AttendanceResponse>.ValidationError(ex.Message);
        }
    }

    private static ApplicationResult<AttendanceResponse> ToAttendanceResponseResult(ApplicationResult<bool> validationResult) =>
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
            attendance.PatientId,
            attendance.AttendanceNumber,
            attendance.OpenedAt,
            attendance.ClosedAt,
            attendance.Status,
            attendance.Type);
}
