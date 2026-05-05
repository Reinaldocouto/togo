using Togo.Application.Tutors.Contracts;
using Togo.Application.Tutors.Validators;
using Togo.Domain.Entities;

namespace Togo.Application.Tutors.UseCases;

public class CreateTutorUseCase
{
    private readonly ITutorRepository _tutorRepository;
    private readonly TutorDocumentUniquenessValidator _documentUniquenessValidator;

    public CreateTutorUseCase(
        ITutorRepository tutorRepository,
        TutorDocumentUniquenessValidator documentUniquenessValidator)
    {
        _tutorRepository = tutorRepository;
        _documentUniquenessValidator = documentUniquenessValidator;
    }

    public async Task<ApplicationResult<TutorResponse>> ExecuteAsync(CreateTutorRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApplicationResult<TutorResponse>.ValidationError("Name is required.");
        }

        var documentUniquenessValidation = await _documentUniquenessValidator
            .ValidateAsync(request.Document, null, cancellationToken);

        if (!documentUniquenessValidation.IsSuccess)
        {
            return ApplicationResult<TutorResponse>.Conflict(documentUniquenessValidation.Error!);
        }

        var tutor = Tutor.Create(request.Name, request.Document, request.Email, request.Phone, DateTime.UtcNow);
        await _tutorRepository.AddAsync(tutor, cancellationToken);

        return ApplicationResult<TutorResponse>.Success(TutorMappings.ToResponse(tutor));
    }
}
