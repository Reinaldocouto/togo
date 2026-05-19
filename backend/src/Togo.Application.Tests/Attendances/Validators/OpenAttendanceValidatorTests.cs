using Togo.Application.Attendances.Validators;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;

namespace Togo.Application.Tests.Attendances.Validators;

public sealed class OpenAttendanceValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid(long patientId)
    {
        var repository = new FakeAttendanceRepository();
        var logger = new TestLogger<OpenAttendanceValidator>();
        var validator = new OpenAttendanceValidator(repository, logger);

        var result = await validator.ValidateAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Patient id is invalid.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnConflict_WhenPatientAlreadyHasOpenAttendance()
    {
        var repository = new FakeAttendanceRepository();
        repository.AddOpenAttendancePatient(10);
        var logger = new TestLogger<OpenAttendanceValidator>();
        var validator = new OpenAttendanceValidator(repository, logger);

        var result = await validator.ValidateAsync(10, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Patient already has an open attendance.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenPatientDoesNotHaveOpenAttendance()
    {
        var repository = new FakeAttendanceRepository();
        repository.AddOpenAttendancePatient(10);
        var logger = new TestLogger<OpenAttendanceValidator>();
        var validator = new OpenAttendanceValidator(repository, logger);

        var result = await validator.ValidateAsync(11, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }
}
