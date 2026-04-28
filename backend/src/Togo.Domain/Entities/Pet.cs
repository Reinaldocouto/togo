using Togo.Domain.Enums;

namespace Togo.Domain.Entities;

public class Pet
{
    private Pet() { }

    private Pet(long patientId, long tutorId, string species, string? breed, PetSex sex, decimal? weightKg, string? microchip)
    {
        ValidateId(patientId, nameof(patientId));
        ValidateId(tutorId, nameof(tutorId));
        ValidateRequired(species, nameof(species));
        ValidateWeight(weightKg);

        PatientId = patientId;
        TutorId = tutorId;
        Species = species.Trim();
        Breed = NormalizeOptional(breed);
        Sex = sex;
        WeightKg = weightKg;
        Microchip = NormalizeOptional(microchip);
    }

    public long PatientId { get; private set; }
    public long TutorId { get; private set; }
    public string Species { get; private set; } = string.Empty;
    public string? Breed { get; private set; }
    public PetSex Sex { get; private set; }
    public decimal? WeightKg { get; private set; }
    public string? Microchip { get; private set; }

    public static Pet Create(long patientId, long tutorId, string species, string? breed, PetSex sex, decimal? weightKg, string? microchip) =>
        new(patientId, tutorId, species, breed, sex, weightKg, microchip);

    public void UpdateProfile(string species, string? breed, PetSex sex, decimal? weightKg, string? microchip)
    {
        ValidateRequired(species, nameof(species));
        ValidateWeight(weightKg);

        Species = species.Trim();
        Breed = NormalizeOptional(breed);
        Sex = sex;
        WeightKg = weightKg;
        Microchip = NormalizeOptional(microchip);
    }

    private static void ValidateId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Id must be greater than zero");
        }
    }

    private static void ValidateRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required", paramName);
        }
    }

    private static void ValidateWeight(decimal? weightKg)
    {
        if (weightKg.HasValue && weightKg.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weightKg), "Weight must be greater than zero");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
