using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Xunit;

namespace Togo.Domain.Tests;

public class PetTests
{
    [Fact]
    public void Create_ShouldCreatePet_WhenDataIsValid()
    {
        // Act
        var pet = Pet.Create(
            patientId: 10,
            tutorId: 1,
            species: "  Dog  ",
            breed: "  Labrador  ",
            sex: PetSex.Male,
            weightKg: 18.5m,
            microchip: "  ABC123  ");

        // Assert
        Assert.Equal(10, pet.PatientId);
        Assert.Equal(1, pet.TutorId);
        Assert.Equal("Dog", pet.Species);
        Assert.Equal("Labrador", pet.Breed);
        Assert.Equal(PetSex.Male, pet.Sex);
        Assert.Equal(18.5m, pet.WeightKg);
        Assert.Equal("ABC123", pet.Microchip);
    }

    [Fact]
    public void Create_ShouldCreatePet_WhenOptionalFieldsAreNull()
    {
        // Act
        var pet = Pet.Create(
            patientId: 10,
            tutorId: 1,
            species: "Dog",
            breed: null,
            sex: PetSex.NotInformed,
            weightKg: null,
            microchip: null);

        // Assert
        Assert.Equal(10, pet.PatientId);
        Assert.Equal(1, pet.TutorId);
        Assert.Equal("Dog", pet.Species);
        Assert.Null(pet.Breed);
        Assert.Equal(PetSex.NotInformed, pet.Sex);
        Assert.Null(pet.WeightKg);
        Assert.Null(pet.Microchip);
    }

    [Fact]
    public void Create_ShouldNormalizeOptionalFieldsToNull_WhenOptionalFieldsAreEmpty()
    {
        // Act
        var pet = Pet.Create(
            patientId: 10,
            tutorId: 1,
            species: "Dog",
            breed: "   ",
            sex: PetSex.Male,
            weightKg: null,
            microchip: "   ");

        // Assert
        Assert.Null(pet.Breed);
        Assert.Null(pet.Microchip);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenPatientIdIsInvalid(long patientId)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Pet.Create(
                patientId: patientId,
                tutorId: 1,
                species: "Dog",
                breed: "Labrador",
                sex: PetSex.Male,
                weightKg: 18.5m,
                microchip: "ABC123"));
        Assert.StartsWith("Id must be greater than zero", exception.Message);
        Assert.Equal("patientId", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenTutorIdIsInvalid(long tutorId)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Pet.Create(
                patientId: 10,
                tutorId: tutorId,
                species: "Dog",
                breed: "Labrador",
                sex: PetSex.Male,
                weightKg: 18.5m,
                microchip: "ABC123"));
        Assert.StartsWith("Id must be greater than zero", exception.Message);
        Assert.Equal("tutorId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenSpeciesIsEmpty(string species)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Pet.Create(
                patientId: 10,
                tutorId: 1,
                species: species,
                breed: "Labrador",
                sex: PetSex.Male,
                weightKg: 18.5m,
                microchip: "ABC123"));
        Assert.StartsWith("Value is required", exception.Message);
        Assert.Equal("species", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenWeightIsZero()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Pet.Create(
                patientId: 10,
                tutorId: 1,
                species: "Dog",
                breed: "Labrador",
                sex: PetSex.Male,
                weightKg: 0,
                microchip: "ABC123"));
        Assert.StartsWith("Weight must be greater than zero", exception.Message);
        Assert.Equal("weightKg", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenWeightIsNegative()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Pet.Create(
                patientId: 10,
                tutorId: 1,
                species: "Dog",
                breed: "Labrador",
                sex: PetSex.Male,
                weightKg: -1,
                microchip: "ABC123"));
        Assert.StartsWith("Weight must be greater than zero", exception.Message);
        Assert.Equal("weightKg", exception.ParamName);
    }

    [Fact]
    public void UpdateProfile_ShouldUpdatePet_WhenDataIsValid()
    {
        // Arrange
        var pet = CreateValidPet();

        // Act
        pet.UpdateProfile(
            species: "  Cat  ",
            breed: "  Siamese  ",
            sex: PetSex.Female,
            weightKg: 4.2m,
            microchip: "  XYZ789  ");

        // Assert
        Assert.Equal("Cat", pet.Species);
        Assert.Equal("Siamese", pet.Breed);
        Assert.Equal(PetSex.Female, pet.Sex);
        Assert.Equal(4.2m, pet.WeightKg);
        Assert.Equal("XYZ789", pet.Microchip);
        Assert.Equal(10, pet.PatientId);
        Assert.Equal(1, pet.TutorId);
    }

    [Fact]
    public void UpdateProfile_ShouldAllowNullOptionalFields()
    {
        // Arrange
        var pet = CreateValidPet();

        // Act
        pet.UpdateProfile(
            species: "Dog",
            breed: null,
            sex: PetSex.Male,
            weightKg: null,
            microchip: null);

        // Assert
        Assert.Null(pet.Breed);
        Assert.Null(pet.WeightKg);
        Assert.Null(pet.Microchip);
    }

    [Fact]
    public void UpdateProfile_ShouldNormalizeOptionalFieldsToNull_WhenOptionalFieldsAreEmpty()
    {
        // Arrange
        var pet = CreateValidPet();

        // Act
        pet.UpdateProfile(
            species: "Dog",
            breed: "   ",
            sex: PetSex.Male,
            weightKg: 18.5m,
            microchip: "   ");

        // Assert
        Assert.Null(pet.Breed);
        Assert.Null(pet.Microchip);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_ShouldThrowArgumentException_WhenSpeciesIsEmpty(string species)
    {
        // Arrange
        var pet = CreateValidPet();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            pet.UpdateProfile(
                species: species,
                breed: "Labrador",
                sex: PetSex.Male,
                weightKg: 18.5m,
                microchip: "ABC123"));
        Assert.StartsWith("Value is required", exception.Message);
        Assert.Equal("species", exception.ParamName);
    }

    [Fact]
    public void UpdateProfile_ShouldThrowArgumentOutOfRangeException_WhenWeightIsZero()
    {
        // Arrange
        var pet = CreateValidPet();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            pet.UpdateProfile(
                species: "Dog",
                breed: "Labrador",
                sex: PetSex.Male,
                weightKg: 0,
                microchip: "ABC123"));
        Assert.StartsWith("Weight must be greater than zero", exception.Message);
        Assert.Equal("weightKg", exception.ParamName);
    }

    [Fact]
    public void UpdateProfile_ShouldThrowArgumentOutOfRangeException_WhenWeightIsNegative()
    {
        // Arrange
        var pet = CreateValidPet();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            pet.UpdateProfile(
                species: "Dog",
                breed: "Labrador",
                sex: PetSex.Male,
                weightKg: -1,
                microchip: "ABC123"));
        Assert.StartsWith("Weight must be greater than zero", exception.Message);
        Assert.Equal("weightKg", exception.ParamName);
    }

    private static Pet CreateValidPet() =>
        Pet.Create(
            patientId: 10,
            tutorId: 1,
            species: "Dog",
            breed: "Labrador",
            sex: PetSex.Male,
            weightKg: 18.5m,
            microchip: "ABC123");
}
