using Togo.Domain.Enums;

namespace Togo.Application.Pets.Contracts;

public record PetListItemResponse(
    long PatientId,
    long TutorId,
    string Name,
    string Species,
    string? Breed,
    PetSex Sex,
    string Status,
    string? Microchip);
