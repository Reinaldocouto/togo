using Microsoft.Extensions.Logging;
using Togo.Application.Tutors.Contracts;
using Togo.Application.Tutors.Validators;

namespace Togo.Application.Tutors.UseCases;

public class UpdateTutorUseCase
{
    private readonly ITutorRepository _tutorRepository;
    private readonly TutorDocumentUniquenessValidator _documentUniquenessValidator;
    private readonly ILogger<UpdateTutorUseCase> _logger;

    public UpdateTutorUseCase(
        ITutorRepository tutorRepository,
        TutorDocumentUniquenessValidator documentUniquenessValidator,
        ILogger<UpdateTutorUseCase> logger)
    {
        _tutorRepository = tutorRepository;
        _documentUniquenessValidator = documentUniquenessValidator;
        _logger = logger;
    }

    public async Task<ApplicationResult<TutorResponse>> ExecuteAsync(long id, UpdateTutorRequest request, CancellationToken cancellationToken)
    {
        var hasDocument = !string.IsNullOrWhiteSpace(request.Document);
        _logger.LogInformation("Updating tutor. TutorId: {TutorId}. HasDocument: {HasDocument}", id, hasDocument);

        if (id <= 0)
        {
            _logger.LogWarning("Tutor update failed due to invalid id. TutorId: {TutorId}", id);
            return ApplicationResult<TutorResponse>.ValidationError("Invalid tutor id.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Tutor update failed due to validation error. TutorId: {TutorId}", id);
            return ApplicationResult<TutorResponse>.ValidationError("Name is required.");
        }

        var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken);
        if (tutor is null)
        {
            _logger.LogWarning("Tutor update failed because tutor was not found. TutorId: {TutorId}", id);
            return ApplicationResult<TutorResponse>.NotFound("Tutor not found.");
        }

        var documentUniquenessValidation = await _documentUniquenessValidator
            .ValidateAsync(request.Document, id, cancellationToken);

        if (!documentUniquenessValidation.IsSuccess)
        {
            _logger.LogWarning("Tutor update blocked due to duplicated document. TutorId: {TutorId}", id);
            return ApplicationResult<TutorResponse>.Conflict(documentUniquenessValidation.Error!);
        }

        var updatedAt = DateTime.UtcNow;
        tutor.UpdateName(request.Name, updatedAt);
        tutor.UpdateContact(request.Document, request.Email, request.Phone, updatedAt);

        await _tutorRepository.UpdateAsync(tutor, cancellationToken);

        _logger.LogInformation("Tutor updated successfully. TutorId: {TutorId}", id);

        return ApplicationResult<TutorResponse>.Success(TutorMappings.ToResponse(tutor));
    }
}
