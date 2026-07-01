using Microsoft.EntityFrameworkCore;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;

namespace Togo.Infrastructure.Repositories;

public class TutorRepository : ITutorRepository
{
    private readonly AppDbContext _context;

    public TutorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Tutor?> GetByIdAsync(long id, long clinicId, CancellationToken cancellationToken)
    {
        return await _context.Tutors.FirstOrDefaultAsync(t => t.Id == id && t.ClinicId == clinicId, cancellationToken);
    }

    public async Task<IReadOnlyList<Tutor>> ListAsync(long clinicId, CancellationToken cancellationToken)
    {
        return await _context.Tutors
            .AsNoTracking()
            .Where(t => t.ClinicId == clinicId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Tutor tutor, CancellationToken cancellationToken)
    {
        await _context.Tutors.AddAsync(tutor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tutor tutor, CancellationToken cancellationToken)
    {
        _context.Tutors.Update(tutor);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Tutor tutor, CancellationToken cancellationToken)
    {
        _context.Tutors.Remove(tutor);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Tutor cannot be deleted because it is related to other records.");
        }
    }

    public async Task<bool> ExistsByDocumentAsync(long clinicId, string document, long? ignoreTutorId, CancellationToken cancellationToken)
    {
        var normalizedDocument = document.Trim();

        return await _context.Tutors
            .AsNoTracking()
            .AnyAsync(t => t.ClinicId == clinicId && t.Document == normalizedDocument && (!ignoreTutorId.HasValue || t.Id != ignoreTutorId.Value), cancellationToken);
    }
}
