using Microsoft.Extensions.Logging;
using Togo.Application.Tutors;

namespace Togo.Application.Pets.Validators;

public class PetMicrochipUniquenessValidator
{
    private readonly IPetRepository _petRepository;
    private readonly ILogger<PetMicrochipUniquenessValidator> _logger;

    public PetMicrochipUniquenessValidator(
        IPetRepository petRepository,
        ILogger<PetMicrochipUniquenessValidator> logger)
    {
        _petRepository = petRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<bool>> ValidateAsync(
        string? microchip,
        long? ignorePatientId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(microchip))
        {
            _logger.LogInformation("Pet microchip uniqueness validation skipped because microchip was not provided");
            return ApplicationResult<bool>.Success(true);
        }

        var normalizedMicrochip = microchip.Trim();
        var microchipExists = await _petRepository.MicrochipExistsAsync(normalizedMicrochip, ignorePatientId, cancellationToken);
        if (microchipExists)
        {
            _logger.LogWarning(
                "Pet microchip uniqueness validation failed. HasMicrochip: {HasMicrochip}. IgnorePatientId: {IgnorePatientId}",
                true,
                ignorePatientId);
            return ApplicationResult<bool>.Conflict("A pet with this microchip already exists.");
        }

        _logger.LogDebug(
            "Pet microchip uniqueness validation succeeded. HasMicrochip: {HasMicrochip}. IgnorePatientId: {IgnorePatientId}",
            true,
            ignorePatientId);

        return ApplicationResult<bool>.Success(true);
    }
}
