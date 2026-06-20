using Togo.Domain.Entities;

namespace Togo.Application.ClinicalEvolutions.Repositories;

public interface IClinicalEvolutionRepository
{
    Task AddAsync(ClinicalEvolution clinicalEvolution, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClinicalEvolution>> ListByAttendanceIdAsync(long attendanceId, CancellationToken cancellationToken = default);
}
