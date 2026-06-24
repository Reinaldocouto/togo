using Microsoft.EntityFrameworkCore;
using Togo.Application.Prescriptions.Repositories;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;

namespace Togo.Infrastructure.Repositories;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly AppDbContext _context;

    public PrescriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Prescription prescription, IReadOnlyList<PrescriptionItemDraft> items, CancellationToken cancellationToken = default)
    {
        await _context.Prescriptions.AddAsync(prescription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var prescriptionItems = items
            .Select(item => PrescriptionItem.Create(prescription.Id, item.ProductId, item.Quantity, item.Unit, item.Dosage, item.DurationDays))
            .ToList();

        await _context.PrescriptionItems.AddRangeAsync(prescriptionItems, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PrescriptionListItemProjection>> ListByAttendanceIdAsync(long attendanceId, CancellationToken cancellationToken = default)
    {
        return await _context.Prescriptions
            .AsNoTracking()
            .Where(prescription => prescription.AttendanceId == attendanceId)
            .GroupJoin(
                _context.PrescriptionItems.AsNoTracking(),
                prescription => prescription.Id,
                item => item.PrescriptionId,
                (prescription, items) => new PrescriptionListItemProjection(
                    prescription.Id,
                    prescription.AttendanceId,
                    prescription.IssuedAt,
                    items.Count()))
            .OrderBy(item => item.IssuedAt)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
    }
}
