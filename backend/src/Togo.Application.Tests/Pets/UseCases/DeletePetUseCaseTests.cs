using Microsoft.Extensions.Logging;
using Togo.Application.Pets.UseCases;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;

namespace Togo.Application.Tests.Pets.UseCases;

public sealed class DeletePetUseCaseTests
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
        Assert.False(result.Data);
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
        Assert.False(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenPetExists()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet();
        var useCase = CreateUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRemovePetFromRepository_WhenDeleteSucceeds()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet();
        var useCase = CreateUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(patientId, CancellationToken.None);
        var pet = await repository.GetByPatientIdAsync(patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.Null(pet);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet();
        repository.AddDeleteConflict(patientId);
        var useCase = CreateUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Pet cannot be deleted.", result.Error);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogDeleteSuccess_WhenPetIsDeleted()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet();
        var logger = new TestLogger<DeletePetUseCase>();
        var useCase = CreateUseCase(repository, logger);

        // Act
        var result = await useCase.ExecuteAsync(patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Information);
        Assert.Contains(
            logger.Entries,
            entry => entry.Message.Contains("Pet deleted successfully", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogConflict_WhenDeleteIsBlocked()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet();
        repository.AddDeleteConflict(patientId);
        var logger = new TestLogger<DeletePetUseCase>();
        var useCase = CreateUseCase(repository, logger);

        // Act
        var result = await useCase.ExecuteAsync(patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Warning);
        Assert.Contains(
            logger.Entries,
            entry => entry.Message.Contains("Pet delete blocked due to conflict", StringComparison.Ordinal));
    }

    private static DeletePetUseCase CreateUseCase(
        FakePetRepository repository,
        TestLogger<DeletePetUseCase>? logger = null) =>
        new(repository, logger ?? new TestLogger<DeletePetUseCase>());
}
