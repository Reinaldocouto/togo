using Microsoft.Extensions.Logging;
using Togo.Application.Security;
using Togo.Application.Pets.Contracts;
using Togo.Application.Tutors;

namespace Togo.Application.Pets.UseCases;

public class ListPetsUseCase
{
    private readonly IPetRepository _petRepository;
    private readonly ICurrentClinicalContext _currentClinicalContext;
    private readonly IClinicalContextAuthorizationService _clinicalContextAuthorizationService;
    private readonly ILogger<ListPetsUseCase> _logger;

    public ListPetsUseCase(
        IPetRepository petRepository,
        ICurrentClinicalContext currentClinicalContext,
        IClinicalContextAuthorizationService clinicalContextAuthorizationService,
        ILogger<ListPetsUseCase> logger)
    {
        _petRepository = petRepository;
        _currentClinicalContext = currentClinicalContext;
        _clinicalContextAuthorizationService = clinicalContextAuthorizationService;
        _logger = logger;
    }

    public async Task<ApplicationResult<IReadOnlyList<PetListItemResponse>>> ExecuteAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing pets");

        var clinicId = _currentClinicalContext.GetRequiredClinicId();
        await _clinicalContextAuthorizationService.EnsureCanAccessCurrentClinicAsync(cancellationToken);

        var pets = await _petRepository.ListAsync(clinicId, cancellationToken);
        var responses = PetMappings.ToListItemResponses(pets);

        _logger.LogInformation("Pets listed successfully. Count: {Count}", responses.Count);

        return ApplicationResult<IReadOnlyList<PetListItemResponse>>.Success(responses);
    }
}
