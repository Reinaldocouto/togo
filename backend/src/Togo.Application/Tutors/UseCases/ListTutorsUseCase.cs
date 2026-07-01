using Microsoft.Extensions.Logging;
using Togo.Application.Security;
using Togo.Application.Tutors.Contracts;

namespace Togo.Application.Tutors.UseCases;

public class ListTutorsUseCase
{
    private readonly ITutorRepository _tutorRepository;
    private readonly ICurrentClinicalContext _currentClinicalContext;
    private readonly IClinicalContextAuthorizationService _clinicalContextAuthorizationService;
    private readonly ILogger<ListTutorsUseCase> _logger;

    public ListTutorsUseCase(
        ITutorRepository tutorRepository,
        ICurrentClinicalContext currentClinicalContext,
        IClinicalContextAuthorizationService clinicalContextAuthorizationService,
        ILogger<ListTutorsUseCase> logger)
    {
        _tutorRepository = tutorRepository;
        _currentClinicalContext = currentClinicalContext;
        _clinicalContextAuthorizationService = clinicalContextAuthorizationService;
        _logger = logger;
    }

    public async Task<ApplicationResult<IReadOnlyList<TutorListItemResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing tutors");

        var clinicId = _currentClinicalContext.GetRequiredClinicId();
        await _clinicalContextAuthorizationService.EnsureCanAccessCurrentClinicAsync(cancellationToken);

        var tutors = await _tutorRepository.ListAsync(clinicId, cancellationToken);
        var response = tutors.Select(TutorMappings.ToListItemResponse).ToList().AsReadOnly();

        _logger.LogInformation("Tutors listed successfully. Count: {Count}", response.Count);

        return ApplicationResult<IReadOnlyList<TutorListItemResponse>>.Success(response);
    }
}
