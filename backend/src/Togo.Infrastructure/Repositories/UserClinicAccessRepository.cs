using Microsoft.EntityFrameworkCore;
using Togo.Application.Security;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;

namespace Togo.Infrastructure.Repositories;

public sealed class UserClinicAccessRepository : IUserClinicAccessRepository
{
    private readonly AppDbContext _context;

    public UserClinicAccessRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> HasActiveAccessAsync(Guid userId, long clinicId, CancellationToken cancellationToken = default)
    {
        return _context.UserClinicAccesses
            .AsNoTracking()
            .AnyAsync(access => access.UserId == userId && access.ClinicId == clinicId && access.IsActive, cancellationToken);
    }

    public Task<UserClinicAccess?> GetAsync(Guid userId, long clinicId, CancellationToken cancellationToken = default)
    {
        return _context.UserClinicAccesses
            .AsNoTracking()
            .FirstOrDefaultAsync(access => access.UserId == userId && access.ClinicId == clinicId, cancellationToken);
    }
}
