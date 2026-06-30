using Togo.Application.Attendances.Validators;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;

namespace Togo.Application.Tests.Attendances.Validators;

public sealed class AttendancePatientExistsValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid(long patientId)
    {
        var repository = new FakePetRepository();
        var logger = new TestLogger<AttendancePatientExistsValidator>();
        var validator = new AttendancePatientExistsValidator(repository, logger);

        var result = await validator.ValidateAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Patient id is invalid.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        var repository = new FakePetRepository();
        var logger = new TestLogger<AttendancePatientExistsValidator>();
        var validator = new AttendancePatientExistsValidator(repository, logger);

        var result = await validator.ValidateAsync(999, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Patient not found.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenPatientExists()
    {
        var repository = new FakePetRepository();
        var patientId = repository.AddPet(clinicId: 42);
        var logger = new TestLogger<AttendancePatientExistsValidator>();
        var validator = new AttendancePatientExistsValidator(repository, logger);

        var result = await validator.ValidateAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(patientId, result.Data.PatientId);
        Assert.Equal(42, result.Data.ClinicId);
    }
}
