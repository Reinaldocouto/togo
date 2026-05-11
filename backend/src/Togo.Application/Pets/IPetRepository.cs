using Togo.Application.Pets.Contracts;

namespace Togo.Application.Pets;

public interface IPetRepository
{
    Task<IReadOnlyList<PetListItemProjection>> ListAsync(CancellationToken cancellationToken);

    Task<PetDetailsProjection?> GetByPatientIdAsync(long patientId, CancellationToken cancellationToken);

    Task<bool> TutorExistsAsync(long tutorId, CancellationToken cancellationToken);

    Task<bool> MicrochipExistsAsync(string microchip, long? ignorePatientId, CancellationToken cancellationToken);

    Task<PetDetailsProjection> CreateAsync(CreatePetRepositoryData data, CancellationToken cancellationToken);

    Task<PetDetailsProjection?> UpdateAsync(UpdatePetRepositoryData data, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(long patientId, CancellationToken cancellationToken);
}
