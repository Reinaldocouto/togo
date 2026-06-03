using Togo.Application.MedicalRecords.Repositories;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.MedicalRecords.Fakes;

internal sealed class FakeMedicalRecordRepository : IMedicalRecordRepository
{
    private readonly Dictionary<long, MedicalRecord> _recordsByPatientId = [];
    private long _nextId = 1;

    public int AddCallsCount { get; private set; }
    public int UpdateCallsCount { get; private set; }
    public IReadOnlyCollection<MedicalRecord> Items => _recordsByPatientId.Values;
    public long? LastExistsByPatientIdInput { get; private set; }
    public long? LastGetByPatientIdInput { get; private set; }

    public bool ReturnNullOnGetByPatientId { get; set; }

    public void AddExisting(MedicalRecord medicalRecord)
    {
        AssignIdIfMissing(medicalRecord);
        _recordsByPatientId[medicalRecord.PatientId] = medicalRecord;
    }

    public Task<MedicalRecord?> GetByIdAsync(long id)
    {
        var record = _recordsByPatientId.Values.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(record);
    }

    public Task<MedicalRecord?> GetByPatientIdAsync(long patientId)
    {
        LastGetByPatientIdInput = patientId;

        if (ReturnNullOnGetByPatientId)
        {
            return Task.FromResult<MedicalRecord?>(null);
        }

        _recordsByPatientId.TryGetValue(patientId, out var record);
        return Task.FromResult(record);
    }

    public Task<bool> ExistsByPatientIdAsync(long patientId)
    {
        LastExistsByPatientIdInput = patientId;
        return Task.FromResult(_recordsByPatientId.ContainsKey(patientId));
    }

    public Task AddAsync(MedicalRecord medicalRecord)
    {
        AddCallsCount++;
        AssignIdIfMissing(medicalRecord);
        _recordsByPatientId[medicalRecord.PatientId] = medicalRecord;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(MedicalRecord medicalRecord)
    {
        UpdateCallsCount++;
        _recordsByPatientId[medicalRecord.PatientId] = medicalRecord;
        return Task.CompletedTask;
    }

    private void AssignIdIfMissing(MedicalRecord medicalRecord)
    {
        if (medicalRecord.Id != 0)
        {
            return;
        }

        typeof(MedicalRecord).GetProperty(nameof(MedicalRecord.Id))!
            .SetValue(medicalRecord, _nextId++);
    }
}
