using Togo.Application.Attendances.Repositories;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.Attendances.Fakes;

internal sealed class FakeAttendanceRepository : IAttendanceRepository
{
    private readonly Dictionary<long, Attendance> _itemsById = [];
    private readonly HashSet<string> _existingNumbers = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<long> _patientsWithOpenAttendance = [];
    public string? LastExistsByAttendanceNumberInput { get; private set; }

    public Task<Attendance?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _itemsById.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    public Task<IReadOnlyList<Attendance>> ListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<Attendance>>(_itemsById.Values.ToList());
    }

    public Task<IReadOnlyList<Attendance>> ListByPatientIdAsync(long patientId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var list = _itemsById.Values.Where(a => a.PatientId == patientId).ToList();
        return Task.FromResult<IReadOnlyList<Attendance>>(list);
    }

    public Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _itemsById[attendance.Id] = attendance;
        _existingNumbers.Add(attendance.AttendanceNumber);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _itemsById[attendance.Id] = attendance;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByAttendanceNumberAsync(string attendanceNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastExistsByAttendanceNumberInput = attendanceNumber;
        return Task.FromResult(_existingNumbers.Contains(attendanceNumber));
    }

    public Task<bool> HasOpenAttendanceForPatientAsync(long patientId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_patientsWithOpenAttendance.Contains(patientId));
    }

    public void AddExistingAttendanceNumber(string attendanceNumber) => _existingNumbers.Add(attendanceNumber);
    public void AddOpenAttendancePatient(long patientId) => _patientsWithOpenAttendance.Add(patientId);
}
