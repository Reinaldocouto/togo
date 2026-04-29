using Togo.Application.Tutors.Contracts;

namespace Togo.Application.Tutors.UseCases;

public class UpdateTutorUseCase
{
    private readonly ITutorRepository _tutorRepository;

    public UpdateTutorUseCase(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
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

        if (!string.IsNullOrWhiteSpace(request.Document) &&
            await _tutorRepository.ExistsByDocumentAsync(request.Document, id, cancellationToken))
        {
            return ApplicationResult<TutorResponse>.Conflict("A tutor with this document already exists.");
        }

        var updatedAt = DateTime.UtcNow;
        tutor.UpdateName(request.Name, updatedAt);
        tutor.UpdateContact(request.Document, request.Email, request.Phone, updatedAt);

        await _tutorRepository.UpdateAsync(tutor, cancellationToken);

        return ApplicationResult<TutorResponse>.Success(TutorMappings.ToResponse(tutor));
    }
}
