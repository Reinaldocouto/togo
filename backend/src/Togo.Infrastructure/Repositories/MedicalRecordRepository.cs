using Microsoft.EntityFrameworkCore;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;

namespace Togo.Infrastructure.Repositories;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly AppDbContext _context;

    public MedicalRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MedicalRecord?> GetByIdAsync(long id)
    {
        return await _context.MedicalRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(medicalRecord => medicalRecord.Id == id && !medicalRecord.IsDeleted);
    }

    public async Task<MedicalRecord?> GetByPatientIdAsync(long patientId)
    {
        return await _context.MedicalRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(medicalRecord => medicalRecord.PatientId == patientId && !medicalRecord.IsDeleted);
    }

    public async Task<bool> ExistsByPatientIdAsync(long patientId)
    {
        return await _context.MedicalRecords
            .AsNoTracking()
            .AnyAsync(medicalRecord => medicalRecord.PatientId == patientId && !medicalRecord.IsDeleted);
    }

    public async Task AddAsync(MedicalRecord medicalRecord)
    {
        await _context.MedicalRecords.AddAsync(medicalRecord);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MedicalRecord medicalRecord)
    {
        _context.MedicalRecords.Update(medicalRecord);
        await _context.SaveChangesAsync();
    }
}
