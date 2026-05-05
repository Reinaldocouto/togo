namespace Togo.Application.Tutors.Validators;

public class TutorDocumentUniquenessValidator
{
    private readonly ITutorRepository _tutorRepository;

    public TutorDocumentUniquenessValidator(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    public async Task<ApplicationResult<bool>> ValidateAsync(
        string? document,
        long? ignoreTutorId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return ApplicationResult<bool>.Success(true);
        }

        var documentExists = await _tutorRepository.ExistsByDocumentAsync(document, ignoreTutorId, cancellationToken);
        if (documentExists)
        {
            return ApplicationResult<bool>.Conflict("A tutor with this document already exists.");
        }

        return ApplicationResult<bool>.Success(true);
    }
}
