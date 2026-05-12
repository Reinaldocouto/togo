using Togo.Application.Pets.Contracts;

namespace Togo.Application.Pets;

public static class PetMappings
{
    public static PetResponse ToResponse(PetDetailsProjection pet) =>
        new(
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

    public static PetListItemResponse ToListItemResponse(PetListItemProjection pet) =>
        new(
            pet.PatientId,
            pet.TutorId,
            pet.Name,
            pet.Species,
            pet.Breed,
            pet.Sex,
            pet.Status,
            pet.Microchip);

    public static IReadOnlyList<PetListItemResponse> ToListItemResponses(IReadOnlyList<PetListItemProjection> pets) =>
        pets.Select(ToListItemResponse).ToList().AsReadOnly();
}
