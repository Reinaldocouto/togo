using Microsoft.Extensions.Logging;
using Togo.Application.Tutors;

namespace Togo.Application.Pets.UseCases;

public class DeletePetUseCase
{
    private readonly IPetRepository _petRepository;
    private readonly ILogger<DeletePetUseCase> _logger;

    public DeletePetUseCase(
        IPetRepository petRepository,
        ILogger<DeletePetUseCase> logger)
    {
        _petRepository = petRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<bool>> ExecuteAsync(
        long patientId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting pet. PatientId: {PatientId}", patientId);

        if (patientId <= 0)
        {
            _logger.LogWarning("Pet delete failed due to invalid patient id. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.ValidationError("Patient id is invalid.");
        }

        try
        {
            var deleted = await _petRepository.DeleteAsync(patientId, cancellationToken);
            if (!deleted)
            {
                _logger.LogWarning("Pet delete failed because pet was not found. PatientId: {PatientId}", patientId);
                return ApplicationResult<bool>.NotFound("Pet not found.");
            }

            _logger.LogInformation("Pet deleted successfully. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Pet delete blocked due to conflict. PatientId: {PatientId}", patientId);
            return ApplicationResult<bool>.Conflict(ex.Message);
        }
    }
}
