using Togo.Domain.Enums;

namespace Togo.Application.Pets.Contracts;

public record CreatePetRepositoryData(
    long TutorId,
    PatientType PatientType,
    string Name,
    DateOnly? BirthDate,
    string Status,
    DateTime CreatedAt,
    string Species,
    string? Breed,
    PetSex Sex,
    decimal? WeightKg,
    string? Microchip);
