namespace Togo.Application.Tutors.UseCases;

public class DeleteTutorUseCase
{
    private readonly ITutorRepository _tutorRepository;

    public DeleteTutorUseCase(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    public async Task<ApplicationResult<bool>> ExecuteAsync(long id, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken);
        if (tutor is null)
        {
            return ApplicationResult<bool>.NotFound("Tutor not found.");
        }

        try
        {
            await _tutorRepository.DeleteAsync(tutor, cancellationToken);
            return ApplicationResult<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return ApplicationResult<bool>.Conflict(ex.Message);
        }
    }
}
