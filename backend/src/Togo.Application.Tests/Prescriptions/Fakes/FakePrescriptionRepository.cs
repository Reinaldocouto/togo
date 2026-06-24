using Togo.Application.Prescriptions.Repositories;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.Prescriptions.Fakes;

internal sealed class FakePrescriptionRepository : IPrescriptionRepository
{
    private readonly List<PrescriptionListItemProjection> _listItems = [];

    public int AddCallsCount { get; private set; }
    public int ListByAttendanceIdCallsCount { get; private set; }
    public Prescription? AddedPrescription { get; private set; }
    public IReadOnlyList<PrescriptionItemDraft> AddedItems { get; private set; } = [];

    public Task AddAsync(Prescription prescription, IReadOnlyList<PrescriptionItemDraft> items, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AddCallsCount++;
        AddedPrescription = prescription;
        AddedItems = items.ToList();
        if (prescription.Id == 0)
        {
            typeof(Prescription).GetProperty(nameof(Prescription.Id))!.SetValue(prescription, 1L);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PrescriptionListItemProjection>> ListByAttendanceIdAsync(long attendanceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ListByAttendanceIdCallsCount++;
        return Task.FromResult<IReadOnlyList<PrescriptionListItemProjection>>(_listItems.Where(item => item.AttendanceId == attendanceId).ToList());
    }

    public void AddListItem(PrescriptionListItemProjection item) => _listItems.Add(item);
}
