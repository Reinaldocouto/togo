using Togo.Domain.Enums;

namespace Togo.Application.Pets.Contracts;

public record UpdatePetRepositoryData(
    long PatientId,
    long TutorId,
    string Name,
    DateOnly? BirthDate,
    string Status,
    DateTime UpdatedAt,
    string Species,
    string? Breed,
    PetSex Sex,
    decimal? WeightKg,
    string? Microchip);
