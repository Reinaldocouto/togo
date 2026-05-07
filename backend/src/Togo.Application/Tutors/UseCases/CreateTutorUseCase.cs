using Microsoft.Extensions.Logging;
using Togo.Application.Tutors.Contracts;
using Togo.Application.Tutors.Validators;
using Togo.Domain.Entities;

namespace Togo.Application.Tutors.UseCases;

public class CreateTutorUseCase
{
    private readonly ITutorRepository _tutorRepository;
    private readonly TutorDocumentUniquenessValidator _documentUniquenessValidator;
    private readonly ILogger<CreateTutorUseCase> _logger;

    public CreateTutorUseCase(
        ITutorRepository tutorRepository,
        TutorDocumentUniquenessValidator documentUniquenessValidator,
        ILogger<CreateTutorUseCase> logger)
    {
        _tutorRepository = tutorRepository;
        _documentUniquenessValidator = documentUniquenessValidator;
        _logger = logger;
    }

    public async Task<ApplicationResult<TutorResponse>> ExecuteAsync(CreateTutorRequest request, CancellationToken cancellationToken)
    {
        var hasDocument = !string.IsNullOrWhiteSpace(request.Document);
        _logger.LogInformation("Creating tutor. HasDocument: {HasDocument}", hasDocument);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Tutor creation failed due to validation error");
            return ApplicationResult<TutorResponse>.ValidationError("Name is required.");
        }

        var documentUniquenessValidation = await _documentUniquenessValidator
            .ValidateAsync(request.Document, null, cancellationToken);

        if (!documentUniquenessValidation.IsSuccess)
        {
            _logger.LogWarning("Tutor creation blocked due to duplicated document");
            return ApplicationResult<TutorResponse>.Conflict(documentUniquenessValidation.Error!);
        }

        var tutor = Tutor.Create(request.Name, request.Document, request.Email, request.Phone, DateTime.UtcNow);
        await _tutorRepository.AddAsync(tutor, cancellationToken);

        _logger.LogInformation("Tutor created successfully. TutorId: {TutorId}", tutor.Id);

        return ApplicationResult<TutorResponse>.Success(TutorMappings.ToResponse(tutor));
    }
}
