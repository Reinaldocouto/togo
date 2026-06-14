using Microsoft.EntityFrameworkCore;
using Togo.Application.MedicalRecords.Exceptions;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Repositories.MedicalRecords;

namespace Togo.Infrastructure.Repositories;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly AppDbContext _context;

    public MedicalRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MedicalRecord?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await _context.MedicalRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(medicalRecord => medicalRecord.Id == id && !medicalRecord.IsDeleted, cancellationToken);
    }

    public async Task<MedicalRecord?> GetByPatientIdAsync(long patientId, CancellationToken cancellationToken)
    {
        return await _context.MedicalRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(medicalRecord => medicalRecord.PatientId == patientId && !medicalRecord.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByPatientIdAsync(long patientId, CancellationToken cancellationToken)
    {
        return await _context.MedicalRecords
            .AsNoTracking()
            .AnyAsync(medicalRecord => medicalRecord.PatientId == patientId && !medicalRecord.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsIncludingSoftDeletedByPatientIdAsync(long patientId, CancellationToken cancellationToken)
    {
        return await _context.MedicalRecords
            .AsNoTracking()
            .AnyAsync(medicalRecord => medicalRecord.PatientId == patientId, cancellationToken);
    }

    public async Task AddAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken)
    {
        await _context.MedicalRecords.AddAsync(medicalRecord, cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (MedicalRecordUniqueConstraintDetector.IsMedicalRecordPatientIdUniqueConstraintViolation(ex))
        {
            throw new MedicalRecordAlreadyExistsException(medicalRecord.PatientId, ex);
        }
    }

    public async Task UpdateAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken)
    {
        _context.MedicalRecords.Update(medicalRecord);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
