using Togo.Application.Tutors.Validators;
using Togo.Application.Tutors.Contracts;

namespace Togo.Application.Tutors.UseCases;

public class UpdateTutorUseCase
{
    private readonly ITutorRepository _tutorRepository;
    private readonly TutorDocumentUniquenessValidator _documentUniquenessValidator;

    public UpdateTutorUseCase(
        ITutorRepository tutorRepository,
        TutorDocumentUniquenessValidator documentUniquenessValidator)
    {
        _tutorRepository = tutorRepository;
        _documentUniquenessValidator = documentUniquenessValidator;
    }

    public async Task<ApplicationResult<TutorResponse>> ExecuteAsync(long id, UpdateTutorRequest request, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return ApplicationResult<TutorResponse>.ValidationError("Invalid tutor id.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApplicationResult<TutorResponse>.ValidationError("Name is required.");
        }

        var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken);
        if (tutor is null)
        {
            return ApplicationResult<TutorResponse>.NotFound("Tutor not found.");
        }

        var documentUniquenessValidation = await _documentUniquenessValidator
            .ValidateAsync(request.Document, id, cancellationToken);

        if (!documentUniquenessValidation.IsSuccess)
        {
            return ApplicationResult<TutorResponse>.Conflict(documentUniquenessValidation.Error!);
        }

        var updatedAt = DateTime.UtcNow;
        tutor.UpdateName(request.Name, updatedAt);
        tutor.UpdateContact(request.Document, request.Email, request.Phone, updatedAt);

        await _tutorRepository.UpdateAsync(tutor, cancellationToken);

        return ApplicationResult<TutorResponse>.Success(TutorMappings.ToResponse(tutor));
    }
}
