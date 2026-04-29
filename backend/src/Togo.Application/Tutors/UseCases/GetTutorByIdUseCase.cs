using Togo.Application.Tutors.Contracts;

namespace Togo.Application.Tutors.UseCases;

public class GetTutorByIdUseCase
{
    private readonly ITutorRepository _tutorRepository;

    public GetTutorByIdUseCase(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    public async Task<ApplicationResult<TutorResponse>> ExecuteAsync(long id, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken);
        if (tutor is null)
        {
            return ApplicationResult<TutorResponse>.NotFound("Tutor not found.");
        }

        return ApplicationResult<TutorResponse>.Success(TutorMappings.ToResponse(tutor));
    }
}
