using Microsoft.Extensions.Logging;
using Togo.Application.Pets.Contracts;
using Togo.Application.Pets.UseCases;
using Togo.Application.Pets.Validators;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Pets.UseCases;

public sealed class CreatePetUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddExistingTutor(1);
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest();

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.PatientId > 0);
        Assert.Equal(request.TutorId, result.Data.TutorId);
        Assert.Equal(request.Name, result.Data.Name);
        Assert.Equal(request.Status, result.Data.Status);
        Assert.Equal(request.Species, result.Data.Species);
        Assert.Equal(request.Breed, result.Data.Breed);
        Assert.Equal(request.Sex, result.Data.Sex);
        Assert.Equal(request.WeightKg, result.Data.WeightKg);
        Assert.Equal(request.Microchip, result.Data.Microchip);
        Assert.NotEqual(default, result.Data.CreatedAt);
        Assert.Null(result.Data.UpdatedAt);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPersistPetInRepository_WhenRequestIsValid()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddExistingTutor(1);
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest();

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);
        var pet = await repository.GetByPatientIdAsync(result.Data!.PatientId, CancellationToken.None);

        // Assert
        Assert.NotNull(pet);
        Assert.Equal(result.Data.PatientId, pet.PatientId);
        Assert.Equal(request.Name, pet.Name);
        Assert.Equal(request.TutorId, pet.TutorId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenTutorDoesNotExist()
    {
        // Arrange
        var repository = new FakePetRepository();
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest(tutorId: 999);

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Tutor not found.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenMicrochipAlreadyExists()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddExistingTutor(1);
        repository.AddPet(microchip: "MICROCHIP-001");
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest(microchip: "MICROCHIP-001");

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("A pet with this microchip already exists.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowNullMicrochip()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddExistingTutor(1);
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest(microchip: null);

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.Microchip);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotLogMicrochipValue_WhenCreatingPet()
    {
        // Arrange
        const string sensitiveMicrochip = "MICROCHIP-SENSITIVE-001";
        var repository = new FakePetRepository();
        repository.AddExistingTutor(1);
        var logger = new TestLogger<CreatePetUseCase>();
        var useCase = CreateUseCase(repository, logger);
        var request = CreateValidRequest(microchip: sensitiveMicrochip);

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(
            logger.Entries,
            entry => entry.Message.Contains(sensitiveMicrochip, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogCreationSuccess_WhenRequestIsValid()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddExistingTutor(1);
        var logger = new TestLogger<CreatePetUseCase>();
        var useCase = CreateUseCase(repository, logger);
        var request = CreateValidRequest();

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Information);
        Assert.Contains(
            logger.Entries,
            entry => entry.Message.Contains("Pet created successfully", StringComparison.Ordinal));
    }

    private static CreatePetUseCase CreateUseCase(
        FakePetRepository repository,
        TestLogger<CreatePetUseCase>? logger = null)
    {
        var tutorExistsValidator = new PetTutorExistsValidator(
            repository,
            new TestLogger<PetTutorExistsValidator>());
        var microchipUniquenessValidator = new PetMicrochipUniquenessValidator(
            repository,
            new TestLogger<PetMicrochipUniquenessValidator>());

        return new CreatePetUseCase(
            repository,
            tutorExistsValidator,
            microchipUniquenessValidator,
            logger ?? new TestLogger<CreatePetUseCase>());
    }

    private static CreatePetRequest CreateValidRequest(
        long tutorId = 1,
        string name = "Thor",
        DateOnly? birthDate = null,
        string status = "Active",
        string species = "Dog",
        string? breed = "SRD",
        PetSex sex = PetSex.Male,
        decimal? weightKg = 10.5m,
        string? microchip = "MICROCHIP-001") =>
        new(
            tutorId,
            name,
            birthDate,
            status,
            species,
            breed,
            sex,
            weightKg,
            microchip);
}
