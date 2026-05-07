using Microsoft.Extensions.Logging;

namespace Togo.Application.Tutors.UseCases;

public class DeleteTutorUseCase
{
    private readonly ITutorRepository _tutorRepository;
    private readonly ILogger<DeleteTutorUseCase> _logger;

    public DeleteTutorUseCase(
        ITutorRepository tutorRepository,
        ILogger<DeleteTutorUseCase> logger)
    {
        _tutorRepository = tutorRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<bool>> ExecuteAsync(long id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting tutor. TutorId: {TutorId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Tutor delete failed due to invalid id. TutorId: {TutorId}", id);
        }

        var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken);
        if (tutor is null)
        {
            _logger.LogWarning("Tutor delete failed because tutor was not found. TutorId: {TutorId}", id);
            return ApplicationResult<bool>.NotFound("Tutor not found.");
        }

        try
        {
            await _tutorRepository.DeleteAsync(tutor, cancellationToken);
            _logger.LogInformation("Tutor deleted successfully. TutorId: {TutorId}", id);
            return ApplicationResult<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Tutor delete blocked due to conflict. TutorId: {TutorId}", id);
            return ApplicationResult<bool>.Conflict(ex.Message);
        }
    }
}
