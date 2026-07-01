using Microsoft.Extensions.Logging;
using Togo.Application.Security;
using Togo.Application.Pets.Contracts;
using Togo.Application.Pets.Validators;
using Togo.Application.Tutors;

namespace Togo.Application.Pets.UseCases;

public class UpdatePetUseCase
{
    private readonly IPetRepository _petRepository;
    private readonly ICurrentClinicalContext _currentClinicalContext;
    private readonly IClinicalContextAuthorizationService _clinicalContextAuthorizationService;
    private readonly PetTutorExistsValidator _petTutorExistsValidator;
    private readonly PetMicrochipUniquenessValidator _petMicrochipUniquenessValidator;
    private readonly ILogger<UpdatePetUseCase> _logger;

    public UpdatePetUseCase(
        IPetRepository petRepository,
        PetTutorExistsValidator petTutorExistsValidator,
        PetMicrochipUniquenessValidator petMicrochipUniquenessValidator,
        ICurrentClinicalContext currentClinicalContext,
        IClinicalContextAuthorizationService clinicalContextAuthorizationService,
        ILogger<UpdatePetUseCase> logger)
    {
        _petRepository = petRepository;
        _petTutorExistsValidator = petTutorExistsValidator;
        _petMicrochipUniquenessValidator = petMicrochipUniquenessValidator;
        _currentClinicalContext = currentClinicalContext;
        _clinicalContextAuthorizationService = clinicalContextAuthorizationService;
        _logger = logger;
    }

    public async Task<ApplicationResult<PetResponse>> ExecuteAsync(
        long patientId,
        UpdatePetRequest request,
        CancellationToken cancellationToken)
    {
        var hasMicrochip = !string.IsNullOrWhiteSpace(request.Microchip);
        _logger.LogInformation(
            "Updating pet. PatientId: {PatientId}. TutorId: {TutorId}. HasMicrochip: {HasMicrochip}",
            patientId,
            request.TutorId,
            hasMicrochip);

        if (patientId <= 0)
        {
            _logger.LogWarning("Pet update failed due to invalid patient id. PatientId: {PatientId}", patientId);
            return ApplicationResult<PetResponse>.ValidationError("Patient id is invalid.");
        }

        var clinicId = _currentClinicalContext.GetRequiredClinicId();
        await _clinicalContextAuthorizationService.EnsureCanAccessCurrentClinicAsync(cancellationToken);

        var existingPet = await _petRepository.GetByPatientIdAsync(patientId, clinicId, cancellationToken);
        if (existingPet is null)
        {
            _logger.LogWarning("Pet update failed because pet was not found. PatientId: {PatientId}", patientId);
            return ApplicationResult<PetResponse>.NotFound("Pet not found.");
        }

        var tutorValidation = await _petTutorExistsValidator.ValidateAsync(request.TutorId, clinicId, cancellationToken);
        if (!tutorValidation.IsSuccess)
        {
            _logger.LogWarning(
                "Pet update failed because tutor validation did not succeed. PatientId: {PatientId}. TutorId: {TutorId}",
                patientId,
                request.TutorId);
            return ToPetResponseResult(tutorValidation);
        }

        var microchipValidation = await _petMicrochipUniquenessValidator
            .ValidateAsync(request.Microchip, patientId, cancellationToken);
        if (!microchipValidation.IsSuccess)
        {
            _logger.LogWarning(
                "Pet update failed because microchip validation did not succeed. PatientId: {PatientId}. HasMicrochip: {HasMicrochip}",
                patientId,
                hasMicrochip);
            return ToPetResponseResult(microchipValidation);
        }

        var data = new UpdatePetRepositoryData(
            patientId,
            request.TutorId,
            request.Name,
            request.BirthDate,
            request.Status,
            DateTime.UtcNow,
            request.Species,
            request.Breed,
            request.Sex,
            request.WeightKg,
            request.Microchip);

        var updatedPet = await _petRepository.UpdateAsync(data, clinicId, cancellationToken);
        if (updatedPet is null)
        {
            _logger.LogWarning("Pet update failed because pet was not found during repository update. PatientId: {PatientId}", patientId);
            return ApplicationResult<PetResponse>.NotFound("Pet not found.");
        }

        var response = PetMappings.ToResponse(updatedPet);

        _logger.LogInformation(
            "Pet updated successfully. PatientId: {PatientId}. TutorId: {TutorId}. HasMicrochip: {HasMicrochip}",
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
