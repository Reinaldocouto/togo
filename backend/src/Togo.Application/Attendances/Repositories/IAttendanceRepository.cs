using Togo.Domain.Entities;

namespace Togo.Application.Attendances.Repositories;

public interface IAttendanceRepository
{
    Task<Attendance?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Attendance>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Attendance>> ListByPatientIdAsync(long patientId, CancellationToken cancellationToken = default);
    Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default);
    Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default);
    Task<bool> ExistsByAttendanceNumberAsync(string attendanceNumber, CancellationToken cancellationToken = default);
    Task<bool> HasOpenAttendanceForPatientAsync(long patientId, CancellationToken cancellationToken = default);
}
