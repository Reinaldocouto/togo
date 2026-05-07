using Microsoft.Extensions.Logging;
using Togo.Application.Tutors.Contracts;

namespace Togo.Application.Tutors.UseCases;

public class ListTutorsUseCase
{
    private readonly ITutorRepository _tutorRepository;
    private readonly ILogger<ListTutorsUseCase> _logger;

    public ListTutorsUseCase(
        ITutorRepository tutorRepository,
        ILogger<ListTutorsUseCase> logger)
    {
        _tutorRepository = tutorRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<IReadOnlyList<TutorListItemResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing tutors");

        var tutors = await _tutorRepository.ListAsync(cancellationToken);
        var response = tutors.Select(TutorMappings.ToListItemResponse).ToList().AsReadOnly();

        _logger.LogInformation("Tutors listed successfully. Count: {Count}", response.Count);

        return ApplicationResult<IReadOnlyList<TutorListItemResponse>>.Success(response);
    }
}
