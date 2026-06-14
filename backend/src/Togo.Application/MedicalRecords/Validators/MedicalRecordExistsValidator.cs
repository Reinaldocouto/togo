using Microsoft.Extensions.Logging;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Application.Tutors;

namespace Togo.Application.MedicalRecords.Validators;

public class MedicalRecordExistsValidator
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly ILogger<MedicalRecordExistsValidator> _logger;

    public MedicalRecordExistsValidator(
        IMedicalRecordRepository medicalRecordRepository,
        ILogger<MedicalRecordExistsValidator> logger)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<bool>> ValidateAsync(long patientId, CancellationToken cancellationToken)
    {
        if (patientId <= 0)
        {
            _logger.LogWarning("Medical record existence validation failed because patient id is invalid. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.ValidationError("Patient id is invalid.");
        }

        var exists = await _medicalRecordRepository.ExistsByPatientIdAsync(patientId, cancellationToken);
        if (!exists)
        {
            _logger.LogWarning("Medical record existence validation failed because medical record was not found for patient. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.NotFound("Medical record not found.");
        }

        _logger.LogDebug("Medical record existence validation succeeded. PatientId: {PatientId}", patientId);
        return ApplicationResult<bool>.Success(true);
    }
}
