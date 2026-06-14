using Togo.Application.MedicalRecords.Exceptions;
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
    public CancellationToken LastGetByPatientIdCancellationToken { get; private set; }
    public CancellationToken LastExistsByPatientIdCancellationToken { get; private set; }
    public CancellationToken LastExistsIncludingSoftDeletedByPatientIdCancellationToken { get; private set; }
    public CancellationToken LastAddCancellationToken { get; private set; }
    public CancellationToken LastUpdateCancellationToken { get; private set; }

    public bool ReturnNullOnGetByPatientId { get; set; }
    public bool ThrowMedicalRecordAlreadyExistsOnAdd { get; set; }
    public Exception? ExceptionToThrowOnAdd { get; set; }

    public void AddExisting(MedicalRecord medicalRecord)
    {
        AssignIdIfMissing(medicalRecord);
        _records.Add(medicalRecord);
    }

    public Task<MedicalRecord?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var record = _records.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        return Task.FromResult(record);
    }

    public Task<MedicalRecord?> GetByPatientIdAsync(long patientId, CancellationToken cancellationToken)
    {
        LastGetByPatientIdInput = patientId;
        LastGetByPatientIdCancellationToken = cancellationToken;

        if (ReturnNullOnGetByPatientId)
        {
            return Task.FromResult<MedicalRecord?>(null);
        }

        var record = _records.FirstOrDefault(x => x.PatientId == patientId && !x.IsDeleted);
        return Task.FromResult(record);
    }

    public Task<bool> ExistsByPatientIdAsync(long patientId, CancellationToken cancellationToken)
    {
        LastExistsByPatientIdInput = patientId;
        LastExistsByPatientIdCancellationToken = cancellationToken;
        return Task.FromResult(_records.Any(x => x.PatientId == patientId && !x.IsDeleted));
    }

    public Task<bool> ExistsIncludingSoftDeletedByPatientIdAsync(long patientId, CancellationToken cancellationToken)
    {
        LastExistsIncludingSoftDeletedByPatientIdInput = patientId;
        LastExistsIncludingSoftDeletedByPatientIdCancellationToken = cancellationToken;
        return Task.FromResult(_records.Any(x => x.PatientId == patientId));
    }

    public Task AddAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken)
    {
        AddCallsCount++;
        LastAddCancellationToken = cancellationToken;

        if (ThrowMedicalRecordAlreadyExistsOnAdd)
        {
            throw new MedicalRecordAlreadyExistsException(medicalRecord.PatientId, new InvalidOperationException("Simulated unique constraint violation."));
        }

        if (ExceptionToThrowOnAdd is not null)
        {
            throw ExceptionToThrowOnAdd;
        }

        AssignIdIfMissing(medicalRecord);
        _records.Add(medicalRecord);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken)
    {
        UpdateCallsCount++;
        LastUpdateCancellationToken = cancellationToken;
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
