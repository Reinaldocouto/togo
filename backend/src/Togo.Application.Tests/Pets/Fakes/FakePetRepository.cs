using Togo.Application.Pets;
using Togo.Application.Pets.Contracts;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Pets.Fakes;

internal sealed class FakePetRepository : IPetRepository
{
    private readonly List<PetState> _pets = [];
    private readonly Dictionary<long, long> _existingTutorClinics = [];
    private readonly HashSet<long> _deleteConflictPatientIds = [];
    private long _nextPatientId = 1;

    public void AddExistingTutor(long tutorId, long clinicId = 1)
    {
        _existingTutorClinics[tutorId] = clinicId;
    }

    public long AddPet(
        long tutorId = 1,
        string name = "Thor",
        DateOnly? birthDate = null,
        string status = "Active",
        string species = "Dog",
        string? breed = "SRD",
        PetSex sex = PetSex.Male,
        decimal? weightKg = 10.5m,
        string? microchip = "MICROCHIP-001",
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        AddExistingTutor(tutorId);

        var patientId = GetNextPatientId();
        _pets.Add(new PetState(
            patientId,
            tutorId,
            1,
            PatientType.Pet,
            name,
            birthDate,
            status,
            species,
            breed,
            sex,
            weightKg,
            microchip,
            createdAt ?? DateTime.UtcNow,
            updatedAt));

        return patientId;
    }

    public void AddDeleteConflict(long patientId)
    {
        _deleteConflictPatientIds.Add(patientId);
    }

    public Task<IReadOnlyList<PetListItemProjection>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<PetListItemProjection> pets = _pets
            .OrderBy(pet => pet.Name, StringComparer.Ordinal)
            .Select(ToListItemProjection)
            .ToList();

        return Task.FromResult(pets);
    }

    public Task<PetDetailsProjection?> GetByPatientIdAsync(long patientId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var pet = _pets.FirstOrDefault(pet => pet.PatientId == patientId);
        return Task.FromResult(pet is null ? null : ToDetailsProjection(pet));
    }

    public Task<bool> TutorExistsAsync(long tutorId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_existingTutorClinics.ContainsKey(tutorId));
    }

    public Task<bool> TutorBelongsToClinicAsync(long tutorId, long clinicId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_existingTutorClinics.TryGetValue(tutorId, out var tutorClinicId) && tutorClinicId == clinicId);
    }

    public Task<bool> MicrochipExistsAsync(string microchip, long? ignorePatientId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(microchip))
        {
            return Task.FromResult(false);
        }

        var normalizedMicrochip = microchip.Trim();
        var exists = _pets.Any(pet =>
            pet.PatientId != ignorePatientId &&
            !string.IsNullOrWhiteSpace(pet.Microchip) &&
            string.Equals(pet.Microchip.Trim(), normalizedMicrochip, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(exists);
    }

    public Task<PetDetailsProjection> CreateAsync(CreatePetRepositoryData data, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var pet = new PetState(
            GetNextPatientId(),
            data.TutorId,
            data.ClinicId,
            data.PatientType,
            data.Name,
            data.BirthDate,
            data.Status,
            data.Species,
            data.Breed,
            data.Sex,
            data.WeightKg,
            data.Microchip,
            data.CreatedAt,
            UpdatedAt: null);

        _pets.Add(pet);

        return Task.FromResult(ToDetailsProjection(pet));
    }

    public Task<PetDetailsProjection?> UpdateAsync(UpdatePetRepositoryData data, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var petIndex = _pets.FindIndex(pet => pet.PatientId == data.PatientId);
        if (petIndex < 0)
        {
            return Task.FromResult<PetDetailsProjection?>(null);
        }

        var currentPet = _pets[petIndex];
        var updatedPet = currentPet with
        {
            TutorId = data.TutorId,
            Name = data.Name,
            BirthDate = data.BirthDate,
            Status = data.Status,
            Species = data.Species,
            Breed = data.Breed,
            Sex = data.Sex,
            WeightKg = data.WeightKg,
            Microchip = data.Microchip,
            UpdatedAt = data.UpdatedAt
        };

        _pets[petIndex] = updatedPet;

        return Task.FromResult<PetDetailsProjection?>(ToDetailsProjection(updatedPet));
    }

    public Task<bool> DeleteAsync(long patientId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_deleteConflictPatientIds.Contains(patientId))
        {
            throw new InvalidOperationException("Pet cannot be deleted.");
        }

        var petIndex = _pets.FindIndex(pet => pet.PatientId == patientId);
        if (petIndex < 0)
        {
            return Task.FromResult(false);
        }

        _pets.RemoveAt(petIndex);
        _deleteConflictPatientIds.Remove(patientId);

        return Task.FromResult(true);
    }

    private long GetNextPatientId()
    {
        return _nextPatientId++;
    }

    private static PetListItemProjection ToListItemProjection(PetState pet)
    {
        return new PetListItemProjection(
            pet.PatientId,
            pet.TutorId,
            pet.Name,
            pet.Species,
            pet.Breed,
            pet.Sex,
            pet.Status,
            pet.Microchip);
    }

    private static PetDetailsProjection ToDetailsProjection(PetState pet)
    {
        return new PetDetailsProjection(
            pet.PatientId,
            pet.TutorId,
            pet.Name,
            pet.BirthDate,
            pet.Status,
            pet.Species,
            pet.Breed,
            pet.Sex,
            pet.WeightKg,
            pet.Microchip,
            pet.CreatedAt,
            pet.UpdatedAt);
    }

    private sealed record PetState(
        long PatientId,
        long TutorId,
        long ClinicId,
        PatientType PatientType,
        string Name,
        DateOnly? BirthDate,
        string Status,
        string Species,
        string? Breed,
        PetSex Sex,
        decimal? WeightKg,
        string? Microchip,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
