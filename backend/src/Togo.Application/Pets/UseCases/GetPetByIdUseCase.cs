using Microsoft.Extensions.Logging;
using Togo.Application.Security;
using Togo.Application.Pets.Contracts;
using Togo.Application.Tutors;

namespace Togo.Application.Pets.UseCases;

public class GetPetByIdUseCase
{
    private readonly IPetRepository _petRepository;
    private readonly ICurrentClinicalContext _currentClinicalContext;
    private readonly IClinicalContextAuthorizationService _clinicalContextAuthorizationService;
    private readonly ILogger<GetPetByIdUseCase> _logger;

    public GetPetByIdUseCase(
        IPetRepository petRepository,
        ICurrentClinicalContext currentClinicalContext,
        IClinicalContextAuthorizationService clinicalContextAuthorizationService,
        ILogger<GetPetByIdUseCase> logger)
    {
        _petRepository = petRepository;
        _currentClinicalContext = currentClinicalContext;
        _clinicalContextAuthorizationService = clinicalContextAuthorizationService;
        _logger = logger;
    }

    public async Task<ApplicationResult<PetResponse>> ExecuteAsync(
        long patientId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting pet by patient id. PatientId: {PatientId}", patientId);

        if (patientId <= 0)
        {
            _logger.LogWarning("Pet get by patient id failed due to invalid patient id. PatientId: {PatientId}", patientId);
            return ApplicationResult<PetResponse>.ValidationError("Patient id is invalid.");
        }

        var clinicId = _currentClinicalContext.GetRequiredClinicId();
        await _clinicalContextAuthorizationService.EnsureCanAccessCurrentClinicAsync(cancellationToken);

        var pet = await _petRepository.GetByPatientIdAsync(patientId, clinicId, cancellationToken);
        if (pet is null)
        {
            _logger.LogWarning("Pet not found. PatientId: {PatientId}", patientId);
            return ApplicationResult<PetResponse>.NotFound("Pet not found.");
        }

        var response = PetMappings.ToResponse(pet);

        _logger.LogInformation("Pet found. PatientId: {PatientId}", response.PatientId);

        return ApplicationResult<PetResponse>.Success(response);
    }
}
