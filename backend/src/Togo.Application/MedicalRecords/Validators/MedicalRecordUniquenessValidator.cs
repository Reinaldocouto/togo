using Microsoft.Extensions.Logging;
using Togo.Application.MedicalRecords.Exceptions;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Application.Tutors;

namespace Togo.Application.MedicalRecords.Validators;

public class MedicalRecordUniquenessValidator
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly ILogger<MedicalRecordUniquenessValidator> _logger;

    public MedicalRecordUniquenessValidator(
        IMedicalRecordRepository medicalRecordRepository,
        ILogger<MedicalRecordUniquenessValidator> logger)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<bool>> ValidateAsync(long patientId, CancellationToken cancellationToken)
    {
        if (patientId <= 0)
        {
            _logger.LogWarning("Medical record uniqueness validation failed because patient id is invalid. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.ValidationError("Patient id is invalid.");
        }

        var exists = await _medicalRecordRepository.ExistsIncludingSoftDeletedByPatientIdAsync(patientId);
        if (exists)
        {
            _logger.LogWarning("Medical record uniqueness validation failed because patient already has a medical record. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.Conflict(MedicalRecordAlreadyExistsException.DefaultMessage);
        }

        _logger.LogDebug("Medical record uniqueness validation succeeded. PatientId: {PatientId}", patientId);
        return ApplicationResult<bool>.Success(true);
    }
}
