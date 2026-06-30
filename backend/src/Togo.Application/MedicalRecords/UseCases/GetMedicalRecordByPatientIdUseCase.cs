using Microsoft.Extensions.Logging;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.MedicalRecords.UseCases;

public class GetMedicalRecordByPatientIdUseCase
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly MedicalRecordPatientExistsValidator _medicalRecordPatientExistsValidator;
    private readonly MedicalRecordExistsValidator _medicalRecordExistsValidator;
    private readonly ILogger<GetMedicalRecordByPatientIdUseCase> _logger;

    public GetMedicalRecordByPatientIdUseCase(
        IMedicalRecordRepository medicalRecordRepository,
        MedicalRecordPatientExistsValidator medicalRecordPatientExistsValidator,
        MedicalRecordExistsValidator medicalRecordExistsValidator,
        ILogger<GetMedicalRecordByPatientIdUseCase> logger)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _medicalRecordPatientExistsValidator = medicalRecordPatientExistsValidator;
        _medicalRecordExistsValidator = medicalRecordExistsValidator;
        _logger = logger;
    }

    public async Task<ApplicationResult<MedicalRecordResponse>> ExecuteAsync(
        long patientId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting medical record by patient id. PatientId: {PatientId}", patientId);

        var patientValidation = await _medicalRecordPatientExistsValidator.ValidateAsync(patientId, cancellationToken);
        if (!patientValidation.IsSuccess)
        {
            _logger.LogWarning("Medical record query failed because patient validation did not succeed. PatientId: {PatientId}", patientId);
            return ToMedicalRecordResponseResult(patientValidation);
        }

        var medicalRecordValidation = await _medicalRecordExistsValidator.ValidateAsync(patientId, cancellationToken);
        if (!medicalRecordValidation.IsSuccess)
        {
            _logger.LogWarning("Medical record query failed because existence validation did not succeed. PatientId: {PatientId}", patientId);
            return ToMedicalRecordResponseResult(medicalRecordValidation);
        }

        var medicalRecord = await _medicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
        if (medicalRecord is null)
        {
            _logger.LogWarning("Medical record was not found after successful validations. PatientId: {PatientId}", patientId);
            return ApplicationResult<MedicalRecordResponse>.NotFound("Medical record not found.");
        }

        _logger.LogInformation("Medical record found successfully. PatientId: {PatientId}. MedicalRecordId: {MedicalRecordId}", patientId, medicalRecord.Id);
        return ApplicationResult<MedicalRecordResponse>.Success(ToResponse(medicalRecord));
    }

    private static ApplicationResult<MedicalRecordResponse> ToMedicalRecordResponseResult<T>(ApplicationResult<T> validationResult) =>
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
            medicalRecord.ClinicId,
            medicalRecord.PatientId,
            medicalRecord.GeneralNotes,
            medicalRecord.FlagsJson,
            medicalRecord.UpdatedAt);
}
