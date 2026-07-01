using Microsoft.Extensions.Logging;
using Togo.Application.Tutors;

namespace Togo.Application.Pets.Validators;

public class PetTutorExistsValidator
{
    private readonly IPetRepository _petRepository;
    private readonly ILogger<PetTutorExistsValidator> _logger;

    public PetTutorExistsValidator(
        IPetRepository petRepository,
        ILogger<PetTutorExistsValidator> logger)
    {
        _petRepository = petRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<bool>> ValidateAsync(
        long tutorId,
        long clinicId,
        CancellationToken cancellationToken)
    {
        if (clinicId <= 0)
        {
            _logger.LogWarning("Pet tutor validation failed because clinic id is invalid. ClinicId: {ClinicId}", clinicId);
            return ApplicationResult<bool>.ValidationError("ClinicId must be greater than zero.");
        }

        if (tutorId <= 0)
        {
            _logger.LogWarning("Pet tutor existence validation failed because tutor id is invalid. TutorId: {TutorId}", tutorId);
            return ApplicationResult<bool>.ValidationError("Tutor id is invalid.");
        }

        var tutorExists = await _petRepository.TutorExistsAsync(tutorId, clinicId, cancellationToken);
        if (!tutorExists)
        {
            _logger.LogWarning("Pet tutor existence validation failed because tutor was not found. TutorId: {TutorId}", tutorId);
            return ApplicationResult<bool>.NotFound("Tutor not found.");
        }

        var tutorBelongsToClinic = await _petRepository.TutorBelongsToClinicAsync(tutorId, clinicId, cancellationToken);
        if (!tutorBelongsToClinic)
        {
            _logger.LogWarning("Pet tutor validation failed because tutor belongs to another clinic. TutorId: {TutorId}. ClinicId: {ClinicId}", tutorId, clinicId);
            return ApplicationResult<bool>.ValidationError("Tutor does not belong to the informed clinic.");
        }

        _logger.LogDebug("Pet tutor existence validation succeeded. TutorId: {TutorId}. ClinicId: {ClinicId}", tutorId, clinicId);

        return ApplicationResult<bool>.Success(true);
    }
}
