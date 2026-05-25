using Microsoft.Extensions.Logging;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.MedicalRecords.UseCases;

public class CreateMedicalRecordUseCase
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly MedicalRecordPatientExistsValidator _medicalRecordPatientExistsValidator;
    private readonly MedicalRecordUniquenessValidator _medicalRecordUniquenessValidator;
    private readonly ILogger<CreateMedicalRecordUseCase> _logger;

    public CreateMedicalRecordUseCase(
        IMedicalRecordRepository medicalRecordRepository,
        MedicalRecordPatientExistsValidator medicalRecordPatientExistsValidator,
        MedicalRecordUniquenessValidator medicalRecordUniquenessValidator,
        ILogger<CreateMedicalRecordUseCase> logger)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _medicalRecordPatientExistsValidator = medicalRecordPatientExistsValidator;
        _medicalRecordUniquenessValidator = medicalRecordUniquenessValidator;
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
            var medicalRecord = MedicalRecord.Create(patientId, request.GeneralNotes, request.FlagsJson, DateTime.UtcNow);
            await _medicalRecordRepository.AddAsync(medicalRecord);

            _logger.LogInformation("Medical record created successfully. PatientId: {PatientId}. MedicalRecordId: {MedicalRecordId}", patientId, medicalRecord.Id);
            return ApplicationResult<MedicalRecordResponse>.Success(ToResponse(medicalRecord));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Medical record creation failed due to domain validation error. PatientId: {PatientId}", patientId);
            return ApplicationResult<MedicalRecordResponse>.ValidationError(ex.Message);
        }
    }

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
