using Togo.Domain.Entities;

namespace Togo.Application.MedicalRecords.Repositories;

public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<MedicalRecord?> GetByPatientIdAsync(long patientId, CancellationToken cancellationToken);
    Task<bool> ExistsByPatientIdAsync(long patientId, CancellationToken cancellationToken);
    Task<bool> ExistsIncludingSoftDeletedByPatientIdAsync(long patientId, CancellationToken cancellationToken);
    Task AddAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken);
    Task UpdateAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken);
}
