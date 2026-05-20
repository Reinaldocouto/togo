using Microsoft.EntityFrameworkCore;
using Togo.Application.Attendances.Repositories;
using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Togo.Infrastructure.Persistence;

namespace Togo.Infrastructure.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;

    public AttendanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Attendance?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .AsNoTracking()
            .FirstOrDefaultAsync(attendance => attendance.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Attendance>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .AsNoTracking()
            .OrderByDescending(attendance => attendance.OpenedAt)
            .ThenByDescending(attendance => attendance.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Attendance>> ListByPatientIdAsync(long patientId, CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .AsNoTracking()
            .Where(attendance => attendance.PatientId == patientId)
            .OrderByDescending(attendance => attendance.OpenedAt)
            .ThenByDescending(attendance => attendance.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        await _context.Attendances.AddAsync(attendance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        _context.Attendances.Update(attendance);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByAttendanceNumberAsync(string attendanceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .AsNoTracking()
            .AnyAsync(attendance => attendance.AttendanceNumber == attendanceNumber, cancellationToken);
    }

    public async Task<bool> HasOpenAttendanceForPatientAsync(long patientId, CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .AsNoTracking()
            .AnyAsync(
                attendance => attendance.PatientId == patientId && attendance.Status == AttendanceStatus.Open,
                cancellationToken);
    }
}
