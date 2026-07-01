using Togo.Domain.Entities;

namespace Togo.Application.Security;

public interface IUserClinicAccessRepository
{
    Task<bool> HasActiveAccessAsync(Guid userId, long clinicId, CancellationToken cancellationToken = default);

    Task<UserClinicAccess?> GetAsync(Guid userId, long clinicId, CancellationToken cancellationToken = default);
}
