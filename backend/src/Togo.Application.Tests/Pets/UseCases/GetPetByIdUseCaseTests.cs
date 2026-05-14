using Microsoft.Extensions.Logging;
using Togo.Application.Pets.UseCases;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Pets.UseCases;

public sealed class GetPetByIdUseCaseTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid(long patientId)
    {
        // Arrange
        var repository = new FakePetRepository();
        var useCase = CreateUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Patient id is invalid.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenPetDoesNotExist()
    {
        // Arrange
        var repository = new FakePetRepository();
        var useCase = CreateUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(999, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Pet not found.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenPetExists()
    {
        // Arrange
        const long tutorId = 7;
        const string name = "Nina";
        var repository = new FakePetRepository();
        var patientId = repository.AddPet(tutorId: tutorId, name: name);
        var useCase = CreateUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(patientId, result.Data.PatientId);
        Assert.Equal(name, result.Data.Name);
        Assert.Equal(tutorId, result.Data.TutorId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapPetResponseCorrectly()
    {
        // Arrange
        var birthDate = new DateOnly(2021, 5, 10);
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;
        var repository = new FakePetRepository();
        var patientId = repository.AddPet(
            tutorId: 3,
            name: "Mel",
            birthDate: birthDate,
            status: "Active",
            species: "Dog",
            breed: "Shih Tzu",
            sex: PetSex.Female,
            weightKg: 6.4m,
            microchip: "MICROCHIP-MEL",
            createdAt: createdAt,
            updatedAt: updatedAt);
        var useCase = CreateUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(patientId, result.Data.PatientId);
        Assert.Equal(3, result.Data.TutorId);
        Assert.Equal("Mel", result.Data.Name);
        Assert.Equal(birthDate, result.Data.BirthDate);
        Assert.Equal("Active", result.Data.Status);
        Assert.Equal("Dog", result.Data.Species);
        Assert.Equal("Shih Tzu", result.Data.Breed);
        Assert.Equal(PetSex.Female, result.Data.Sex);
        Assert.Equal(6.4m, result.Data.WeightKg);
        Assert.Equal("MICROCHIP-MEL", result.Data.Microchip);
        Assert.Equal(createdAt, result.Data.CreatedAt);
        Assert.Equal(updatedAt, result.Data.UpdatedAt);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogPetFound_WhenPetExists()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet();
        var logger = new TestLogger<GetPetByIdUseCase>();
        var useCase = CreateUseCase(repository, logger);

        // Act
        var result = await useCase.ExecuteAsync(patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Information);
        Assert.Contains(
            logger.Entries,
            entry => entry.Message.Contains("Pet found", StringComparison.Ordinal));
    }

    private static GetPetByIdUseCase CreateUseCase(
        FakePetRepository repository,
        TestLogger<GetPetByIdUseCase>? logger = null) =>
        new(repository, logger ?? new TestLogger<GetPetByIdUseCase>());
}
