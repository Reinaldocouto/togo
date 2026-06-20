using Microsoft.EntityFrameworkCore;
using Togo.Application.ClinicalEvolutions.Repositories;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;

namespace Togo.Infrastructure.Repositories;

public class ClinicalEvolutionRepository : IClinicalEvolutionRepository
{
    private readonly AppDbContext _context;

    public ClinicalEvolutionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ClinicalEvolution clinicalEvolution, CancellationToken cancellationToken = default)
    {
        await _context.ClinicalEvolutions.AddAsync(clinicalEvolution, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClinicalEvolution>> ListByAttendanceIdAsync(long attendanceId, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicalEvolutions
            .AsNoTracking()
            .Where(evolution => evolution.AttendanceId == attendanceId)
            .OrderBy(evolution => evolution.RegisteredAt)
            .ThenBy(evolution => evolution.Id)
            .ToListAsync(cancellationToken);
    }
}
