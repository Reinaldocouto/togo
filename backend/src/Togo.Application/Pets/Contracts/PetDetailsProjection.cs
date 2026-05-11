using Togo.Domain.Enums;

namespace Togo.Application.Pets.Contracts;

public record PetDetailsProjection(
    long PatientId,
    long TutorId,
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
