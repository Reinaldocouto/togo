using Microsoft.Extensions.Logging;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.Pets;
using Togo.Application.Tutors;

namespace Togo.Application.MedicalRecords.Validators;

public class MedicalRecordPatientExistsValidator
{
    private readonly IPetRepository _petRepository;
    private readonly ILogger<MedicalRecordPatientExistsValidator> _logger;

    public MedicalRecordPatientExistsValidator(
        IPetRepository petRepository,
        ILogger<MedicalRecordPatientExistsValidator> logger)
    {
        _petRepository = petRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<MedicalRecordPatientScope>> ValidateAsync(long patientId, CancellationToken cancellationToken)
    {
        if (patientId <= 0)
        {
            _logger.LogWarning("Medical record patient existence validation failed because patient id is invalid. PatientId: {PatientId}", patientId);
            return ApplicationResult<MedicalRecordPatientScope>.ValidationError("Patient id is invalid.");
        }

        var patient = await _petRepository.GetByPatientIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            _logger.LogWarning("Medical record patient existence validation failed because patient was not found. PatientId: {PatientId}", patientId);
            return ApplicationResult<MedicalRecordPatientScope>.NotFound("Patient not found.");
        }

        _logger.LogDebug("Medical record patient existence validation succeeded. PatientId: {PatientId}", patientId);
        return ApplicationResult<MedicalRecordPatientScope>.Success(new MedicalRecordPatientScope(patient.PatientId, patient.ClinicId));
    }
}
