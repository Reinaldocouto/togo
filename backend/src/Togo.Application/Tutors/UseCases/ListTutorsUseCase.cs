using Togo.Application.Tutors.Contracts;

namespace Togo.Application.Tutors.UseCases;

public class ListTutorsUseCase
{
    private readonly ITutorRepository _tutorRepository;

    public ListTutorsUseCase(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    public async Task<ApplicationResult<IReadOnlyList<TutorListItemResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var tutors = await _tutorRepository.ListAsync(cancellationToken);
        var response = tutors.Select(TutorMappings.ToListItemResponse).ToList().AsReadOnly();
        return ApplicationResult<IReadOnlyList<TutorListItemResponse>>.Success(response);
    }
}
