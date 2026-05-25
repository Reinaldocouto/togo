using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;

namespace Togo.Application.Tests.MedicalRecords.Validators;

public sealed class MedicalRecordPatientExistsValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid(long patientId)
    {
        var validator = new MedicalRecordPatientExistsValidator(new FakePetRepository(), new TestLogger<MedicalRecordPatientExistsValidator>());

        var result = await validator.ValidateAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Patient id is invalid.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        var validator = new MedicalRecordPatientExistsValidator(new FakePetRepository(), new TestLogger<MedicalRecordPatientExistsValidator>());

        var result = await validator.ValidateAsync(999, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Patient not found.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenPatientExists()
    {
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var validator = new MedicalRecordPatientExistsValidator(petRepository, new TestLogger<MedicalRecordPatientExistsValidator>());

        var result = await validator.ValidateAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }
}
