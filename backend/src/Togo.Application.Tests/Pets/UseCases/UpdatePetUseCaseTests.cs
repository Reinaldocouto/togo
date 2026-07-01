using Togo.Application.Tests;
using Microsoft.Extensions.Logging;
using Togo.Application.Pets.Contracts;
using Togo.Application.Pets.UseCases;
using Togo.Application.Pets.Validators;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Pets.UseCases;

public sealed class UpdatePetUseCaseTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid(long patientId)
    {
        // Arrange
        var repository = new FakePetRepository();
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest();

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

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
        repository.AddExistingTutor(1);
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest();

        // Act
        var result = await useCase.ExecuteAsync(999, request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Pet not found.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenTutorDoesNotExist()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet(tutorId: 1);
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest(tutorId: 999);

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Tutor not found.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenMicrochipAlreadyExistsInAnotherPet()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddExistingTutor(1);
        repository.AddPet(microchip: "MICROCHIP-001");
        var patientId = repository.AddPet(microchip: "MICROCHIP-002");
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest(microchip: "MICROCHIP-001");

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("A pet with this microchip already exists.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddExistingTutor(1);
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var patientId = repository.AddPet(createdAt: createdAt);
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest();

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(patientId, result.Data.PatientId);
        Assert.Equal(request.TutorId, result.Data.TutorId);
        Assert.Equal(request.Name, result.Data.Name);
        Assert.Equal(request.Status, result.Data.Status);
        Assert.Equal(request.Species, result.Data.Species);
        Assert.Equal(request.Breed, result.Data.Breed);
        Assert.Equal(request.Sex, result.Data.Sex);
        Assert.Equal(request.WeightKg, result.Data.WeightKg);
        Assert.Equal(request.Microchip, result.Data.Microchip);
        Assert.Equal(createdAt, result.Data.CreatedAt);
        Assert.NotNull(result.Data.UpdatedAt);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPersistUpdatedPetInRepository_WhenRequestIsValid()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet();
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest();

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);
        var pet = await repository.GetByPatientIdAsync(patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(pet);
        Assert.Equal(patientId, pet.PatientId);
        Assert.Equal(request.Name, pet.Name);
        Assert.Equal(request.TutorId, pet.TutorId);
        Assert.Equal(request.Microchip, pet.Microchip);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowSameMicrochipForSamePatient()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet(microchip: "MICROCHIP-001");
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest(microchip: "MICROCHIP-001");

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("MICROCHIP-001", result.Data.Microchip);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowNullMicrochip()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet(microchip: "MICROCHIP-001");
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest(microchip: null);

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.Microchip);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotLogMicrochipValue_WhenUpdatingPet()
    {
        // Arrange
        const string sensitiveMicrochip = "MICROCHIP-SENSITIVE-UPDATE-001";
        var repository = new FakePetRepository();
        var patientId = repository.AddPet();
        var logger = new TestLogger<UpdatePetUseCase>();
        var useCase = CreateUseCase(repository, logger);
        var request = CreateValidRequest(microchip: sensitiveMicrochip);

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(
            logger.Entries,
            entry => entry.Message.Contains(sensitiveMicrochip, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogUpdateSuccess_WhenRequestIsValid()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet();
        var logger = new TestLogger<UpdatePetUseCase>();
        var useCase = CreateUseCase(repository, logger);
        var request = CreateValidRequest();

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Information);
        Assert.Contains(
            logger.Entries,
            entry => entry.Message.Contains("Pet updated successfully", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreserveCreatedAt_WhenUpdatingPet()
    {
        // Arrange
        var repository = new FakePetRepository();
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var patientId = repository.AddPet(createdAt: createdAt);
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest();

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(createdAt, result.Data.CreatedAt);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetUpdatedAt_WhenUpdatingPet()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet(updatedAt: null);
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest();

        // Act
        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.UpdatedAt);
        Assert.NotEqual(default, result.Data.UpdatedAt.Value);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenTutorBelongsToExistingPatientClinic()
    {
        var repository = new FakePetRepository();
        var patientId = repository.AddPet(tutorId: 1, clinicId: 3);
        repository.AddExistingTutor(2, clinicId: 3);
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest(tutorId: 2);

        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.TutorId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenTutorBelongsToAnotherClinic()
    {
        var repository = new FakePetRepository();
        var patientId = repository.AddPet(tutorId: 1, clinicId: 3);
        repository.AddExistingTutor(2, clinicId: 4);
        var useCase = CreateUseCase(repository);
        var request = CreateValidRequest(tutorId: 2);

        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Tutor does not belong to the informed clinic.", result.Error);
    }

    [Fact]
    public void UpdatePetRequest_ShouldNotExposeClinicId()
    {
        var clinicIdProperty = typeof(UpdatePetRequest).GetProperty("ClinicId");

        Assert.Null(clinicIdProperty);
    }

    private static UpdatePetUseCase CreateUseCase(
        FakePetRepository repository,
        TestLogger<UpdatePetUseCase>? logger = null)
    {
        var tutorExistsValidator = new PetTutorExistsValidator(
            repository,
            new TestLogger<PetTutorExistsValidator>());
        var microchipUniquenessValidator = new PetMicrochipUniquenessValidator(
            repository,
            new TestLogger<PetMicrochipUniquenessValidator>());

        return new UpdatePetUseCase(
            repository,
            tutorExistsValidator,
            microchipUniquenessValidator,
            new FakeCurrentClinicalContext(1),
            new FakeClinicalContextAuthorizationService(),
            logger ?? new TestLogger<UpdatePetUseCase>());
    }

    private static UpdatePetRequest CreateValidRequest(
        long tutorId = 1,
        string name = "Thor Atualizado",
        DateOnly? birthDate = null,
        string status = "Active",
        string species = "Dog",
        string? breed = "Golden Retriever",
        PetSex sex = PetSex.Male,
        decimal? weightKg = 30.0m,
        string? microchip = "MICROCHIP-UPDATED-001") =>
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
