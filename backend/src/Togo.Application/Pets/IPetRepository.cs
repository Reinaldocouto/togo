using Togo.Application.Pets.Contracts;

namespace Togo.Application.Pets;

public interface IPetRepository
{
    Task<IReadOnlyList<PetListItemProjection>> ListAsync(long clinicId, CancellationToken cancellationToken);

    Task<PetDetailsProjection?> GetByPatientIdAsync(long patientId, long clinicId, CancellationToken cancellationToken);

    // Compatibility method for clinical flows not migrated in Phase 8.6.1.
    Task<PetDetailsProjection?> GetByPatientIdAsync(long patientId, CancellationToken cancellationToken);

    Task<bool> TutorExistsAsync(long tutorId, long clinicId, CancellationToken cancellationToken);

    Task<bool> TutorBelongsToClinicAsync(long tutorId, long clinicId, CancellationToken cancellationToken);

    Task<bool> MicrochipExistsAsync(string microchip, long? ignorePatientId, CancellationToken cancellationToken);

    Task<PetDetailsProjection> CreateAsync(CreatePetRepositoryData data, CancellationToken cancellationToken);

    Task<PetDetailsProjection?> UpdateAsync(UpdatePetRepositoryData data, long clinicId, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(long patientId, long clinicId, CancellationToken cancellationToken);
}
