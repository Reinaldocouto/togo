using Togo.Application.MedicalRecords.Repositories;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.MedicalRecords.Fakes;

internal sealed class FakeMedicalRecordRepository : IMedicalRecordRepository
{
    private readonly List<MedicalRecord> _records = [];
    private long _nextId = 1;

    public int AddCallsCount { get; private set; }
    public int UpdateCallsCount { get; private set; }
    public IReadOnlyCollection<MedicalRecord> Items => _records;
    public long? LastExistsByPatientIdInput { get; private set; }
    public long? LastExistsIncludingSoftDeletedByPatientIdInput { get; private set; }
    public long? LastGetByPatientIdInput { get; private set; }

    public bool ReturnNullOnGetByPatientId { get; set; }

    public void AddExisting(MedicalRecord medicalRecord)
    {
        AssignIdIfMissing(medicalRecord);
        _records.Add(medicalRecord);
    }

    public Task<MedicalRecord?> GetByIdAsync(long id)
    {
        var record = _records.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        return Task.FromResult(record);
    }

    public Task<MedicalRecord?> GetByPatientIdAsync(long patientId)
    {
        LastGetByPatientIdInput = patientId;

        if (ReturnNullOnGetByPatientId)
        {
            return Task.FromResult<MedicalRecord?>(null);
        }

        var record = _records.FirstOrDefault(x => x.PatientId == patientId && !x.IsDeleted);
        return Task.FromResult(record);
    }

    public Task<bool> ExistsByPatientIdAsync(long patientId)
    {
        LastExistsByPatientIdInput = patientId;
        return Task.FromResult(_records.Any(x => x.PatientId == patientId && !x.IsDeleted));
    }

    public Task<bool> ExistsIncludingSoftDeletedByPatientIdAsync(long patientId)
    {
        LastExistsIncludingSoftDeletedByPatientIdInput = patientId;
        return Task.FromResult(_records.Any(x => x.PatientId == patientId));
    }

    public Task AddAsync(MedicalRecord medicalRecord)
    {
        AddCallsCount++;
        AssignIdIfMissing(medicalRecord);
        _records.Add(medicalRecord);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(MedicalRecord medicalRecord)
    {
        UpdateCallsCount++;
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
