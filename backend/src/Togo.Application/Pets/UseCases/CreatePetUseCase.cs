using Microsoft.Extensions.Logging;
using Togo.Application.Security;
using Togo.Application.Pets.Contracts;
using Togo.Application.Pets.Validators;
using Togo.Application.Tutors;
using Togo.Domain.Enums;

namespace Togo.Application.Pets.UseCases;

public class CreatePetUseCase
{
    private readonly IPetRepository _petRepository;
    private readonly ICurrentClinicalContext _currentClinicalContext;
    private readonly IClinicalContextAuthorizationService _clinicalContextAuthorizationService;
    private readonly PetTutorExistsValidator _petTutorExistsValidator;
    private readonly PetMicrochipUniquenessValidator _petMicrochipUniquenessValidator;
    private readonly ILogger<CreatePetUseCase> _logger;

    public CreatePetUseCase(
        IPetRepository petRepository,
        PetTutorExistsValidator petTutorExistsValidator,
        PetMicrochipUniquenessValidator petMicrochipUniquenessValidator,
        ICurrentClinicalContext currentClinicalContext,
        IClinicalContextAuthorizationService clinicalContextAuthorizationService,
        ILogger<CreatePetUseCase> logger)
    {
        _petRepository = petRepository;
        _petTutorExistsValidator = petTutorExistsValidator;
        _petMicrochipUniquenessValidator = petMicrochipUniquenessValidator;
        _currentClinicalContext = currentClinicalContext;
        _clinicalContextAuthorizationService = clinicalContextAuthorizationService;
        _logger = logger;
    }

    public async Task<ApplicationResult<PetResponse>> ExecuteAsync(
        CreatePetRequest request,
        CancellationToken cancellationToken)
    {
        var hasMicrochip = !string.IsNullOrWhiteSpace(request.Microchip);
        _logger.LogInformation(
            "Creating pet. TutorId: {TutorId}. HasMicrochip: {HasMicrochip}",
            request.TutorId,
            hasMicrochip);

        var clinicId = _currentClinicalContext.GetRequiredClinicId();
        await _clinicalContextAuthorizationService.EnsureCanAccessCurrentClinicAsync(cancellationToken);

        if (request.ClinicId > 0 && request.ClinicId != clinicId)
        {
            _logger.LogWarning("Pet creation failed because request clinic differs from authorized context. RequestClinicId: {RequestClinicId}. ClinicId: {ClinicId}", request.ClinicId, clinicId);
            return ApplicationResult<PetResponse>.ValidationError("ClinicId does not match the authorized clinical context.");
        }

        var tutorValidation = await _petTutorExistsValidator.ValidateAsync(request.TutorId, clinicId, cancellationToken);
        if (!tutorValidation.IsSuccess)
        {
            _logger.LogWarning(
                "Pet creation failed because tutor validation did not succeed. TutorId: {TutorId}",
                request.TutorId);
            return ToPetResponseResult(tutorValidation);
        }

        var microchipValidation = await _petMicrochipUniquenessValidator
            .ValidateAsync(request.Microchip, null, cancellationToken);
        if (!microchipValidation.IsSuccess)
        {
            _logger.LogWarning(
                "Pet creation failed because microchip validation did not succeed. HasMicrochip: {HasMicrochip}",
                hasMicrochip);
            return ToPetResponseResult(microchipValidation);
        }

        var data = new CreatePetRepositoryData(
            clinicId,
            request.TutorId,
            PatientType.Pet,
            request.Name,
            request.BirthDate,
            request.Status,
            DateTime.UtcNow,
            request.Species,
            request.Breed,
            request.Sex,
            request.WeightKg,
            request.Microchip);

        var pet = await _petRepository.CreateAsync(data, cancellationToken);
        var response = PetMappings.ToResponse(pet);

        _logger.LogInformation(
            "Pet created successfully. PatientId: {PatientId}. TutorId: {TutorId}. HasMicrochip: {HasMicrochip}",
            response.PatientId,
            response.TutorId,
            hasMicrochip);

        return ApplicationResult<PetResponse>.Success(response);
    }

    private static ApplicationResult<PetResponse> ToPetResponseResult(ApplicationResult<bool> validationResult) =>
        validationResult.Type switch
        {
            ApplicationResultType.NotFound => ApplicationResult<PetResponse>.NotFound(validationResult.Error!),
            ApplicationResultType.ValidationError => ApplicationResult<PetResponse>.ValidationError(validationResult.Error!),
            ApplicationResultType.Conflict => ApplicationResult<PetResponse>.Conflict(validationResult.Error!),
            _ => ApplicationResult<PetResponse>.Failure(validationResult.Error ?? "Pet validation failed.")
        };
}
