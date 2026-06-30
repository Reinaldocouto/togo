using System.Text.Json;
using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Auditing;
using Togo.Application.ClinicalEvolutions.Contracts;
using Togo.Application.ClinicalEvolutions.Repositories;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.ClinicalEvolutions.UseCases;

public class CreateClinicalEvolutionUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IClinicalEvolutionRepository _clinicalEvolutionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClinicalAuditLogWriter _clinicalAuditLogWriter;
    private readonly ILogger<CreateClinicalEvolutionUseCase> _logger;

    public CreateClinicalEvolutionUseCase(
        IAttendanceRepository attendanceRepository,
        IClinicalEvolutionRepository clinicalEvolutionRepository,
        ICurrentUserService currentUserService,
        IClinicalAuditLogWriter clinicalAuditLogWriter,
        ILogger<CreateClinicalEvolutionUseCase> logger)
    {
        _attendanceRepository = attendanceRepository;
        _clinicalEvolutionRepository = clinicalEvolutionRepository;
        _currentUserService = currentUserService;
        _clinicalAuditLogWriter = clinicalAuditLogWriter;
        _logger = logger;
    }

    public async Task<ApplicationResult<ClinicalEvolutionResponse>> ExecuteAsync(
        long attendanceId,
        CreateClinicalEvolutionRequest request,
        CancellationToken cancellationToken)
    {
        if (attendanceId <= 0)
        {
            return ApplicationResult<ClinicalEvolutionResponse>.ValidationError("Attendance id is invalid.");
        }

        if (request.AttendanceId <= 0)
        {
            return ApplicationResult<ClinicalEvolutionResponse>.ValidationError("Request attendance id is invalid.");
        }

        if (request.AttendanceId != attendanceId)
        {
            return ApplicationResult<ClinicalEvolutionResponse>.ValidationError("Route attendance id must match request attendance id.");
        }

        if (!Enum.IsDefined(request.Type))
        {
            return ApplicationResult<ClinicalEvolutionResponse>.ValidationError("Clinical evolution type is invalid.");
        }

        var attendance = await _attendanceRepository.GetByIdAsync(attendanceId, cancellationToken);
        if (attendance is null)
        {
            return ApplicationResult<ClinicalEvolutionResponse>.NotFound("Attendance not found.");
        }

        if (attendance.Status != AttendanceStatus.Open)
        {
            return ApplicationResult<ClinicalEvolutionResponse>.Conflict("Clinical evolution can only be created for an open attendance.");
        }

        try
        {
            var currentUser = _currentUserService.GetCurrentUser();
            var createdAtUtc = DateTime.UtcNow;
            var clinicalEvolution = ClinicalEvolution.Create(
                attendance.ClinicId,
                request.AttendanceId,
                request.RegisteredAt,
                request.Type,
                request.Text,
                currentUser.UserId,
                createdAtUtc);

            await _clinicalEvolutionRepository.AddAsync(clinicalEvolution, cancellationToken);
            await WriteCreatedAuditLogAsync(clinicalEvolution, currentUser, cancellationToken);

            return ApplicationResult<ClinicalEvolutionResponse>.Success(ToResponse(clinicalEvolution));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Clinical evolution creation failed due to validation error. AttendanceId: {AttendanceId}", attendanceId);
            return ApplicationResult<ClinicalEvolutionResponse>.ValidationError(ex.Message);
        }
    }

    private async Task WriteCreatedAuditLogAsync(ClinicalEvolution clinicalEvolution, CurrentUserInfo currentUser, CancellationToken cancellationToken)
    {
        var auditEvent = new ClinicalAuditEvent(
            EntityName: nameof(ClinicalEvolution),
            EntityId: clinicalEvolution.Id.ToString(),
            Action: ClinicalEvolutionAuditActions.Created,
            UserId: currentUser.UserId,
            UserProfile: currentUser.Profile,
            OccurredAt: DateTime.UtcNow,
            MetadataJson: CreateMetadataJson(clinicalEvolution.ClinicId, clinicalEvolution.AttendanceId, clinicalEvolution.Type));

        await _clinicalAuditLogWriter.WriteAsync(auditEvent, cancellationToken);
    }

    private static string CreateMetadataJson(long clinicId, long attendanceId, EvolutionType type) =>
        JsonSerializer.Serialize(new { ClinicId = clinicId, AttendanceId = attendanceId, Type = type.ToString() });

    private static ClinicalEvolutionResponse ToResponse(ClinicalEvolution clinicalEvolution) =>
        new(
            clinicalEvolution.Id,
            clinicalEvolution.ClinicId,
            clinicalEvolution.AttendanceId,
            clinicalEvolution.RegisteredAt,
            clinicalEvolution.Type,
            clinicalEvolution.Text);
}
