using Microsoft.Extensions.Logging;

namespace Togo.Application.Tutors.Validators;

public class TutorDocumentUniquenessValidator
{
    private readonly ITutorRepository _tutorRepository;
    private readonly ILogger<TutorDocumentUniquenessValidator> _logger;

    public TutorDocumentUniquenessValidator(
        ITutorRepository tutorRepository,
        ILogger<TutorDocumentUniquenessValidator> logger)
    {
        _tutorRepository = tutorRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<bool>> ValidateAsync(
        long clinicId,
        string? document,
        long? ignoreTutorId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            _logger.LogInformation("Tutor document uniqueness validation skipped because document was not provided");
            return ApplicationResult<bool>.Success(true);
        }

        var documentExists = await _tutorRepository.ExistsByDocumentAsync(clinicId, document, ignoreTutorId, cancellationToken);
        if (documentExists)
        {
            _logger.LogWarning("Tutor document uniqueness validation failed. ClinicId: {ClinicId}. IgnoreTutorId: {IgnoreTutorId}", clinicId, ignoreTutorId);
            return ApplicationResult<bool>.Conflict("A tutor with this document already exists.");
        }

        _logger.LogDebug("Tutor document uniqueness validation succeeded. ClinicId: {ClinicId}. IgnoreTutorId: {IgnoreTutorId}", clinicId, ignoreTutorId);

        return ApplicationResult<bool>.Success(true);
    }
}
