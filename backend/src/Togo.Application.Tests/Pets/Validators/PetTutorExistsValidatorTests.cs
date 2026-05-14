using Togo.Application.Pets.Validators;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;

namespace Togo.Application.Tests.Pets.Validators;

public sealed class PetTutorExistsValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_ShouldReturnValidationError_WhenTutorIdIsInvalid(long tutorId)
    {
        // Arrange
        var repository = new FakePetRepository();
        var logger = new TestLogger<PetTutorExistsValidator>();
        var validator = new PetTutorExistsValidator(repository, logger);

        // Act
        var result = await validator.ValidateAsync(tutorId, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Tutor id is invalid.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnNotFound_WhenTutorDoesNotExist()
    {
        // Arrange
        var repository = new FakePetRepository();
        var logger = new TestLogger<PetTutorExistsValidator>();
        var validator = new PetTutorExistsValidator(repository, logger);

        // Act
        var result = await validator.ValidateAsync(999, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Tutor not found.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenTutorExists()
    {
        // Arrange
        var repository = new FakePetRepository();
        repository.AddExistingTutor(1);
        var logger = new TestLogger<PetTutorExistsValidator>();
        var validator = new PetTutorExistsValidator(repository, logger);

        // Act
        var result = await validator.ValidateAsync(1, CancellationToken.None);

        // Assert
        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }
}
