using Microsoft.Extensions.Logging;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.MedicalRecords.UseCases;

public class SoftDeleteMedicalRecordUseCase
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly MedicalRecordPatientExistsValidator _medicalRecordPatientExistsValidator;
    private readonly MedicalRecordExistsValidator _medicalRecordExistsValidator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SoftDeleteMedicalRecordUseCase> _logger;

    public SoftDeleteMedicalRecordUseCase(
        IMedicalRecordRepository medicalRecordRepository,
        MedicalRecordPatientExistsValidator medicalRecordPatientExistsValidator,
        MedicalRecordExistsValidator medicalRecordExistsValidator,
        ICurrentUserService currentUserService,
        ILogger<SoftDeleteMedicalRecordUseCase> logger)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _medicalRecordPatientExistsValidator = medicalRecordPatientExistsValidator;
        _medicalRecordExistsValidator = medicalRecordExistsValidator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApplicationResult<MedicalRecordResponse>> ExecuteAsync(long patientId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Soft deleting medical record. PatientId: {PatientId}", patientId);

        var patientValidation = await _medicalRecordPatientExistsValidator.ValidateAsync(patientId, cancellationToken);
        if (!patientValidation.IsSuccess)
        {
            _logger.LogWarning("Medical record soft delete failed because patient validation did not succeed. PatientId: {PatientId}", patientId);
            return ToMedicalRecordResponseResult(patientValidation);
        }

        var medicalRecordValidation = await _medicalRecordExistsValidator.ValidateAsync(patientId, cancellationToken);
        if (!medicalRecordValidation.IsSuccess)
        {
            _logger.LogWarning("Medical record soft delete failed because existence validation did not succeed. PatientId: {PatientId}", patientId);
            return ToMedicalRecordResponseResult(medicalRecordValidation);
        }

        var medicalRecord = await _medicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
        if (medicalRecord is null)
        {
            _logger.LogWarning("Medical record was not found after successful validations in soft delete flow. PatientId: {PatientId}", patientId);
            return ApplicationResult<MedicalRecordResponse>.NotFound("Medical record not found.");
        }

        var currentUser = _currentUserService.GetCurrentUser();

        try
        {
            medicalRecord.SoftDelete(currentUser.UserId, DateTime.UtcNow);
            await _medicalRecordRepository.UpdateAsync(medicalRecord, cancellationToken);

            _logger.LogInformation("Medical record soft deleted successfully. PatientId: {PatientId}. MedicalRecordId: {MedicalRecordId}", patientId, medicalRecord.Id);
            return ApplicationResult<MedicalRecordResponse>.Success(ToResponse(medicalRecord));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Medical record soft delete failed because record was already deleted. PatientId: {PatientId}", patientId);
            return ApplicationResult<MedicalRecordResponse>.Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Medical record soft delete failed due to domain validation error. PatientId: {PatientId}", patientId);
            return ApplicationResult<MedicalRecordResponse>.ValidationError(ex.Message);
        }
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
