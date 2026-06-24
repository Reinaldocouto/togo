using Togo.Domain.Entities;

namespace Togo.Application.Prescriptions.Repositories;

public interface IPrescriptionRepository
{
    Task AddAsync(Prescription prescription, IReadOnlyList<PrescriptionItemDraft> items, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrescriptionListItemProjection>> ListByAttendanceIdAsync(long attendanceId, CancellationToken cancellationToken = default);
}
