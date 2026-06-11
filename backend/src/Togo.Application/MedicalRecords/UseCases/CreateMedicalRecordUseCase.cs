using System.Text.Json;
using Microsoft.Extensions.Logging;
using Togo.Application.Auditing;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.Exceptions;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.MedicalRecords.UseCases;

public class CreateMedicalRecordUseCase
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly MedicalRecordPatientExistsValidator _medicalRecordPatientExistsValidator;
    private readonly MedicalRecordUniquenessValidator _medicalRecordUniquenessValidator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClinicalAuditLogWriter _clinicalAuditLogWriter;
    private readonly ILogger<CreateMedicalRecordUseCase> _logger;

    public CreateMedicalRecordUseCase(
        IMedicalRecordRepository medicalRecordRepository,
        MedicalRecordPatientExistsValidator medicalRecordPatientExistsValidator,
        MedicalRecordUniquenessValidator medicalRecordUniquenessValidator,
        ICurrentUserService currentUserService,
        IClinicalAuditLogWriter clinicalAuditLogWriter,
        ILogger<CreateMedicalRecordUseCase> logger)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _medicalRecordPatientExistsValidator = medicalRecordPatientExistsValidator;
        _medicalRecordUniquenessValidator = medicalRecordUniquenessValidator;
        _currentUserService = currentUserService;
        _clinicalAuditLogWriter = clinicalAuditLogWriter;
        _logger = logger;
    }

    public async Task<ApplicationResult<MedicalRecordResponse>> ExecuteAsync(
        long patientId,
        CreateMedicalRecordRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating medical record. PatientId: {PatientId}", patientId);

        var patientValidation = await _medicalRecordPatientExistsValidator.ValidateAsync(patientId, cancellationToken);
        if (!patientValidation.IsSuccess)
        {
            _logger.LogWarning("Medical record creation failed because patient validation did not succeed. PatientId: {PatientId}", patientId);
            return ToMedicalRecordResponseResult(patientValidation);
        }

        var uniquenessValidation = await _medicalRecordUniquenessValidator.ValidateAsync(patientId, cancellationToken);
        if (!uniquenessValidation.IsSuccess)
        {
            _logger.LogWarning("Medical record creation failed because uniqueness validation did not succeed. PatientId: {PatientId}", patientId);
            return ToMedicalRecordResponseResult(uniquenessValidation);
        }

        try
        {
            var currentUser = _currentUserService.GetCurrentUser();
            var medicalRecord = MedicalRecord.Create(patientId, request.GeneralNotes, request.FlagsJson, currentUser.UserId, DateTime.UtcNow);
            await _medicalRecordRepository.AddAsync(medicalRecord);
            await WriteCreatedAuditLogAsync(medicalRecord, currentUser, cancellationToken);

            _logger.LogInformation("Medical record created successfully. PatientId: {PatientId}. MedicalRecordId: {MedicalRecordId}", patientId, medicalRecord.Id);
            return ApplicationResult<MedicalRecordResponse>.Success(ToResponse(medicalRecord));
        }
        catch (MedicalRecordAlreadyExistsException)
        {
            _logger.LogWarning("Medical record creation failed because a concurrent insert already created a record. PatientId: {PatientId}", patientId);
            return ApplicationResult<MedicalRecordResponse>.Conflict(MedicalRecordAlreadyExistsException.DefaultMessage);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Medical record creation failed due to domain validation error. PatientId: {PatientId}", patientId);
            return ApplicationResult<MedicalRecordResponse>.ValidationError(ex.Message);
        }
    }

    private async Task WriteCreatedAuditLogAsync(MedicalRecord medicalRecord, CurrentUserInfo currentUser, CancellationToken cancellationToken)
    {
        var auditEvent = new ClinicalAuditEvent(
            EntityName: nameof(MedicalRecord),
            EntityId: medicalRecord.Id.ToString(),
            Action: MedicalRecordAuditActions.Created,
            UserId: currentUser.UserId,
            UserProfile: currentUser.Profile,
            OccurredAt: DateTime.UtcNow,
            MetadataJson: CreateMetadataJson(medicalRecord.PatientId));

        await _clinicalAuditLogWriter.WriteAsync(auditEvent, cancellationToken);
    }

    private static string CreateMetadataJson(long patientId) =>
        JsonSerializer.Serialize(new { PatientId = patientId });

    private static ApplicationResult<MedicalRecordResponse> ToMedicalRecordResponseResult(ApplicationResult<bool> validationResult) =>
        validationResult.Type switch
        {
            ApplicationResultType.NotFound => ApplicationResult<MedicalRecordResponse>.NotFound(validationResult.Error!),
            ApplicationResultType.ValidationError => ApplicationResult<MedicalRecordResponse>.ValidationError(validationResult.Error!),
            ApplicationResultType.Conflict => ApplicationResult<MedicalRecordResponse>.Conflict(validationResult.Error!),
            _ => ApplicationResult<MedicalRecordResponse>.Failure(validationResult.Error ?? "Medical record validation failed.")
        };

    private static MedicalRecordResponse ToResponse(MedicalRecord medicalRecord) =>
        new(
            medicalRecord.Id,
            medicalRecord.PatientId,
            medicalRecord.GeneralNotes,
            medicalRecord.FlagsJson,
            medicalRecord.UpdatedAt);
}
