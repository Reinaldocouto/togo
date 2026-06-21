using Togo.Application.ClinicalEvolutions.Repositories;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.ClinicalEvolutions.Fakes;

internal sealed class FakeClinicalEvolutionRepository : IClinicalEvolutionRepository
{
    private readonly List<ClinicalEvolution> _items = [];

    public IReadOnlyList<ClinicalEvolution> Items => _items;

    public int AddCallsCount { get; private set; }
    public int ListByAttendanceIdCallsCount { get; private set; }

    public Task AddAsync(ClinicalEvolution clinicalEvolution, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AddCallsCount++;
        _items.Add(clinicalEvolution);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ClinicalEvolution>> ListByAttendanceIdAsync(long attendanceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ListByAttendanceIdCallsCount++;
        return Task.FromResult<IReadOnlyList<ClinicalEvolution>>(_items.Where(item => item.AttendanceId == attendanceId).ToList());
    }

    public void AddClinicalEvolution(ClinicalEvolution clinicalEvolution) => _items.Add(clinicalEvolution);
}
