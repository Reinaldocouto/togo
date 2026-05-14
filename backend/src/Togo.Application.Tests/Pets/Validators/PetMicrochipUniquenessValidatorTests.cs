using Microsoft.Extensions.Logging;
using Togo.Application.Pets.Validators;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Pets.Validators;

public sealed class PetMicrochipUniquenessValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenMicrochipIsNullOrWhiteSpace(string? microchip)
    {
        // Arrange
        var repository = new FakePetRepository();
        var logger = new TestLogger<PetMicrochipUniquenessValidator>();
        var validator = new PetMicrochipUniquenessValidator(repository, logger);

        // Act
        var result = await validator.ValidateAsync(microchip, ignorePatientId: null, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenMicrochipDoesNotExist()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddPet(microchip: "MICROCHIP-001");
        var logger = new TestLogger<PetMicrochipUniquenessValidator>();
        var validator = new PetMicrochipUniquenessValidator(repository, logger);

        // Act
        var result = await validator.ValidateAsync("MICROCHIP-002", ignorePatientId: null, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnConflict_WhenMicrochipAlreadyExists()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddPet(microchip: "MICROCHIP-001");
        var logger = new TestLogger<PetMicrochipUniquenessValidator>();
        var validator = new PetMicrochipUniquenessValidator(repository, logger);

        // Act
        var result = await validator.ValidateAsync("MICROCHIP-001", ignorePatientId: null, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("A pet with this microchip already exists.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenMicrochipBelongsToIgnoredPatient()
    {
        // Arrange
        var repository = new FakePetRepository();
        var patientId = repository.AddPet(microchip: "MICROCHIP-001");
        var logger = new TestLogger<PetMicrochipUniquenessValidator>();
        var validator = new PetMicrochipUniquenessValidator(repository, logger);

        // Act
        var result = await validator.ValidateAsync("MICROCHIP-001", patientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnConflict_WhenMicrochipExistsInAnotherPatient()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddPet(microchip: "MICROCHIP-001");
        var secondPatientId = repository.AddPet(microchip: "MICROCHIP-002");
        var logger = new TestLogger<PetMicrochipUniquenessValidator>();
        var validator = new PetMicrochipUniquenessValidator(repository, logger);

        // Act
        var result = await validator.ValidateAsync("MICROCHIP-001", secondPatientId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("A pet with this microchip already exists.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldTrimMicrochipBeforeChecking()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddPet(microchip: "MICROCHIP-001");
        var logger = new TestLogger<PetMicrochipUniquenessValidator>();
        var validator = new PetMicrochipUniquenessValidator(repository, logger);

        // Act
        var result = await validator.ValidateAsync("  MICROCHIP-001  ", ignorePatientId: null, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("A pet with this microchip already exists.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldNotLogMicrochipValue_WhenConflictOccurs()
    {
        // Arrange
        const string sensitiveMicrochip = "MICROCHIP-SENSITIVE-001";
        var repository = new FakePetRepository();
        repository.AddPet(microchip: sensitiveMicrochip);
        var logger = new TestLogger<PetMicrochipUniquenessValidator>();
        var validator = new PetMicrochipUniquenessValidator(repository, logger);

        // Act
        var result = await validator.ValidateAsync(sensitiveMicrochip, ignorePatientId: null, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Warning);
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains(sensitiveMicrochip, StringComparison.Ordinal));
    }
}
