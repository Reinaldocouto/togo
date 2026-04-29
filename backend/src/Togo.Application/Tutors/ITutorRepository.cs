using Togo.Domain.Entities;

namespace Togo.Application.Tutors;

public interface ITutorRepository
{
    Task<Tutor?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Tutor>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(Tutor tutor, CancellationToken cancellationToken);
    Task UpdateAsync(Tutor tutor, CancellationToken cancellationToken);
    Task DeleteAsync(Tutor tutor, CancellationToken cancellationToken);
    Task<bool> ExistsByDocumentAsync(string document, long? ignoreTutorId, CancellationToken cancellationToken);
}
