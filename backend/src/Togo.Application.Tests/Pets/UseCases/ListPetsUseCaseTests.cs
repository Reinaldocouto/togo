using Microsoft.Extensions.Logging;
using Togo.Application.Pets.UseCases;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Pets.UseCases;

public sealed class ListPetsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessWithEmptyList_WhenNoPetsExist()
    {
        // Arrange
        var repository = new FakePetRepository();
        var useCase = CreateUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessWithPetsOrderedByName()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddPet(name: "Thor");
        repository.AddPet(name: "Apolo");
        repository.AddPet(name: "Zeus");
        var useCase = CreateUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count);
        Assert.Equal(new[] { "Apolo", "Thor", "Zeus" }, result.Data.Select(pet => pet.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapListItemResponseCorrectly()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddPet(
            tutorId: 10,
            name: "Luna",
            species: "Cat",
            breed: "SRD",
            sex: PetSex.Female,
            status: "Active",
            microchip: "MICROCHIP-LUNA");
        var useCase = CreateUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var response = Assert.Single(result.Data);
        Assert.True(response.PatientId > 0);
        Assert.Equal(10, response.TutorId);
        Assert.Equal("Luna", response.Name);
        Assert.Equal("Cat", response.Species);
        Assert.Equal("SRD", response.Breed);
        Assert.Equal(PetSex.Female, response.Sex);
        Assert.Equal("Active", response.Status);
        Assert.Equal("MICROCHIP-LUNA", response.Microchip);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogListingSuccess()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddPet(name: "Thor");
        repository.AddPet(name: "Luna");
        var logger = new TestLogger<ListPetsUseCase>();
        var useCase = CreateUseCase(repository, logger);

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Information);
        Assert.Contains(
            logger.Entries,
            entry => entry.Message.Contains("Pets listed successfully", StringComparison.Ordinal));
    }

    private static ListPetsUseCase CreateUseCase(
        FakePetRepository repository,
        TestLogger<ListPetsUseCase>? logger = null) =>
        new(repository, logger ?? new TestLogger<ListPetsUseCase>());
}
