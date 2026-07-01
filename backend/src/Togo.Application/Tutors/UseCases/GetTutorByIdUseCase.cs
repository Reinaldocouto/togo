using Microsoft.Extensions.Logging;
using Togo.Application.Security;
using Togo.Application.Tutors.Contracts;

namespace Togo.Application.Tutors.UseCases;

public class GetTutorByIdUseCase
{
    private readonly ITutorRepository _tutorRepository;
    private readonly ICurrentClinicalContext _currentClinicalContext;
    private readonly IClinicalContextAuthorizationService _clinicalContextAuthorizationService;
    private readonly ILogger<GetTutorByIdUseCase> _logger;

    public GetTutorByIdUseCase(
        ITutorRepository tutorRepository,
        ICurrentClinicalContext currentClinicalContext,
        IClinicalContextAuthorizationService clinicalContextAuthorizationService,
        ILogger<GetTutorByIdUseCase> logger)
    {
        _tutorRepository = tutorRepository;
        _currentClinicalContext = currentClinicalContext;
        _clinicalContextAuthorizationService = clinicalContextAuthorizationService;
        _logger = logger;
    }

    public async Task<ApplicationResult<TutorResponse>> ExecuteAsync(long id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tutor by id. TutorId: {TutorId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Tutor get by id failed due to invalid id. TutorId: {TutorId}", id);
        }

        var clinicId = _currentClinicalContext.GetRequiredClinicId();
        await _clinicalContextAuthorizationService.EnsureCanAccessCurrentClinicAsync(cancellationToken);

        var tutor = await _tutorRepository.GetByIdAsync(id, clinicId, cancellationToken);
        if (tutor is null)
        {
            _logger.LogWarning("Tutor not found. TutorId: {TutorId}", id);
            return ApplicationResult<TutorResponse>.NotFound("Tutor not found.");
        }

        _logger.LogInformation("Tutor found. TutorId: {TutorId}", id);

        return ApplicationResult<TutorResponse>.Success(TutorMappings.ToResponse(tutor));
    }
}
