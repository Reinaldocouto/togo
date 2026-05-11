using Togo.Domain.Enums;

namespace Togo.Application.Pets.Contracts;

public record UpdatePetRequest(
    long TutorId,
    string Name,
    DateOnly? BirthDate,
    string Status,
    string Species,
    string? Breed,
    PetSex Sex,
    decimal? WeightKg,
    string? Microchip);
