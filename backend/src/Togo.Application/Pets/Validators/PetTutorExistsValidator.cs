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
        CancellationToken cancellationToken)
    {
        if (tutorId <= 0)
        {
            _logger.LogWarning("Pet tutor existence validation failed because tutor id is invalid. TutorId: {TutorId}", tutorId);
            return ApplicationResult<bool>.ValidationError("Tutor id is invalid.");
        }

        var tutorExists = await _petRepository.TutorExistsAsync(tutorId, cancellationToken);
        if (!tutorExists)
        {
            _logger.LogWarning("Pet tutor existence validation failed because tutor was not found. TutorId: {TutorId}", tutorId);
            return ApplicationResult<bool>.NotFound("Tutor not found.");
        }

        _logger.LogDebug("Pet tutor existence validation succeeded. TutorId: {TutorId}", tutorId);

        return ApplicationResult<bool>.Success(true);
    }
}
