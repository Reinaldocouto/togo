using Togo.Domain.Entities;

namespace Togo.Application.MedicalRecords.Repositories;

public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(long id);
    Task<MedicalRecord?> GetByPatientIdAsync(long patientId);
    Task<bool> ExistsByPatientIdAsync(long patientId);
    Task AddAsync(MedicalRecord medicalRecord);
    Task UpdateAsync(MedicalRecord medicalRecord);
}
