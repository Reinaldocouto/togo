using Togo.Application.Tutors.Contracts;
using Togo.Domain.Entities;

namespace Togo.Application.Tutors.UseCases;

public class CreateTutorUseCase
{
    private readonly ITutorRepository _tutorRepository;

    public CreateTutorUseCase(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    public async Task<ApplicationResult<TutorResponse>> ExecuteAsync(CreateTutorRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApplicationResult<TutorResponse>.ValidationError("Name is required.");
        }

        if (!string.IsNullOrWhiteSpace(request.Document) &&
            await _tutorRepository.ExistsByDocumentAsync(request.Document, null, cancellationToken))
        {
            return ApplicationResult<TutorResponse>.Conflict("A tutor with this document already exists.");
        }

        var tutor = Tutor.Create(request.Name, request.Document, request.Email, request.Phone, DateTime.UtcNow);
        await _tutorRepository.AddAsync(tutor, cancellationToken);

        return ApplicationResult<TutorResponse>.Success(TutorMappings.ToResponse(tutor));
    }
}
