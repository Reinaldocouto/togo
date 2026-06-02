using Microsoft.Extensions.Logging;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.MedicalRecords.UseCases;

public class UpdateMedicalRecordUseCase
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly MedicalRecordPatientExistsValidator _medicalRecordPatientExistsValidator;
    private readonly MedicalRecordExistsValidator _medicalRecordExistsValidator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateMedicalRecordUseCase> _logger;

    public UpdateMedicalRecordUseCase(
        IMedicalRecordRepository medicalRecordRepository,
        MedicalRecordPatientExistsValidator medicalRecordPatientExistsValidator,
        MedicalRecordExistsValidator medicalRecordExistsValidator,
        ICurrentUserService currentUserService,
        ILogger<UpdateMedicalRecordUseCase> logger)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _medicalRecordPatientExistsValidator = medicalRecordPatientExistsValidator;
        _medicalRecordExistsValidator = medicalRecordExistsValidator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApplicationResult<MedicalRecordResponse>> ExecuteAsync(
        long patientId,
        UpdateMedicalRecordRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating medical record. PatientId: {PatientId}", patientId);

        var patientValidation = await _medicalRecordPatientExistsValidator.ValidateAsync(patientId, cancellationToken);
        if (!patientValidation.IsSuccess)
        {
            _logger.LogWarning("Medical record update failed because patient validation did not succeed. PatientId: {PatientId}", patientId);
            return ToMedicalRecordResponseResult(patientValidation);
        }

        var medicalRecordValidation = await _medicalRecordExistsValidator.ValidateAsync(patientId, cancellationToken);
        if (!medicalRecordValidation.IsSuccess)
        {
            _logger.LogWarning("Medical record update failed because existence validation did not succeed. PatientId: {PatientId}", patientId);
            return ToMedicalRecordResponseResult(medicalRecordValidation);
        }

        var medicalRecord = await _medicalRecordRepository.GetByPatientIdAsync(patientId);
        if (medicalRecord is null)
        {
            _logger.LogWarning("Medical record was not found after successful validations in update flow. PatientId: {PatientId}", patientId);
            return ApplicationResult<MedicalRecordResponse>.NotFound("Medical record not found.");
        }

        try
        {
            var currentUser = _currentUserService.GetCurrentUser();
            medicalRecord.UpdateNotes(request.GeneralNotes, request.FlagsJson, currentUser.UserId, DateTime.UtcNow);
            await _medicalRecordRepository.UpdateAsync(medicalRecord);

            _logger.LogInformation("Medical record updated successfully. PatientId: {PatientId}. MedicalRecordId: {MedicalRecordId}", patientId, medicalRecord.Id);
            return ApplicationResult<MedicalRecordResponse>.Success(ToResponse(medicalRecord));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Medical record update failed due to domain validation error. PatientId: {PatientId}", patientId);
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
