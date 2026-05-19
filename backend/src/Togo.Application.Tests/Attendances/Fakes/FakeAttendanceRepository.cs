using Togo.Application.Attendances.Repositories;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.Attendances.Fakes;

internal sealed class FakeAttendanceRepository : IAttendanceRepository
{
    private readonly List<Attendance> _items = [];
    private readonly Dictionary<long, Attendance> _itemsByLookupId = [];
    private readonly HashSet<string> _existingNumbers = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<long> _patientsWithOpenAttendance = [];
    public string? LastExistsByAttendanceNumberInput { get; private set; }
    public long? LastHasOpenAttendancePatientIdInput { get; private set; }
    public int AddCallsCount { get; private set; }
    public int GetByIdCallsCount { get; private set; }
    public int UpdateCallsCount { get; private set; }
    public Task<Attendance?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GetByIdCallsCount++;

        if (_itemsByLookupId.TryGetValue(id, out var configuredAttendance))
        {
            return Task.FromResult<Attendance?>(configuredAttendance);
        }

        var value = _items.FirstOrDefault(a => a.Id == id);
        return Task.FromResult(value);
    }
    public Task<IReadOnlyList<Attendance>> ListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<Attendance>>(_items.ToList());
    }

    public Task<IReadOnlyList<Attendance>> ListByPatientIdAsync(long patientId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var list = _items.Where(a => a.PatientId == patientId).ToList();
        return Task.FromResult<IReadOnlyList<Attendance>>(list);
    }

    public Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AddCallsCount++;
        _items.Add(attendance);
        _existingNumbers.Add(attendance.AttendanceNumber);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        UpdateCallsCount++;

        foreach (var lookup in _itemsByLookupId.Where(x => ReferenceEquals(x.Value, attendance)).ToList())
        {
            _itemsByLookupId[lookup.Key] = attendance;
        }

        var existingIndex = _items.FindIndex(a => a.Id == attendance.Id);

        if (existingIndex >= 0)
        {
            _items[existingIndex] = attendance;
            return Task.CompletedTask;
        }

        if (!_items.Contains(attendance))
        {
            _items.Add(attendance);
        }

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
        LastHasOpenAttendancePatientIdInput = patientId;
        return Task.FromResult(_patientsWithOpenAttendance.Contains(patientId));
    }

    public void AddExistingAttendanceNumber(string attendanceNumber) => _existingNumbers.Add(attendanceNumber);
    public void AddOpenAttendancePatient(long patientId) => _patientsWithOpenAttendance.Add(patientId);
    public void AddAttendanceForLookup(long id, Attendance attendance) => _itemsByLookupId[id] = attendance;
}
