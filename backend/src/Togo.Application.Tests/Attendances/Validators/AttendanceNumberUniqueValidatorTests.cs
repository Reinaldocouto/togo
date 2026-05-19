using Togo.Application.Attendances.Validators;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;

namespace Togo.Application.Tests.Attendances.Validators;

public sealed class AttendanceNumberUniqueValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_ShouldReturnValidationError_WhenAttendanceNumberIsEmpty(string? attendanceNumber)
    {
        var repository = new FakeAttendanceRepository();
        var logger = new TestLogger<AttendanceNumberUniqueValidator>();
        var validator = new AttendanceNumberUniqueValidator(repository, logger);

        var result = await validator.ValidateAsync(attendanceNumber, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Attendance number is required.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnConflict_WhenAttendanceNumberAlreadyExists()
    {
        var repository = new FakeAttendanceRepository();
        repository.AddExistingAttendanceNumber("ATT-001");
        var logger = new TestLogger<AttendanceNumberUniqueValidator>();
        var validator = new AttendanceNumberUniqueValidator(repository, logger);

        var result = await validator.ValidateAsync("ATT-001", CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("An attendance with this number already exists.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenAttendanceNumberDoesNotExist()
    {
        var repository = new FakeAttendanceRepository();
        repository.AddExistingAttendanceNumber("ATT-001");
        var logger = new TestLogger<AttendanceNumberUniqueValidator>();
        var validator = new AttendanceNumberUniqueValidator(repository, logger);

        var result = await validator.ValidateAsync("ATT-002", CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ValidateAsync_ShouldTrimAttendanceNumberBeforeChecking()
    {
        var repository = new FakeAttendanceRepository();
        repository.AddExistingAttendanceNumber("ATT-001");
        var logger = new TestLogger<AttendanceNumberUniqueValidator>();
        var validator = new AttendanceNumberUniqueValidator(repository, logger);

        var result = await validator.ValidateAsync("  ATT-001  ", CancellationToken.None);

        Assert.Equal("ATT-001", repository.LastExistsByAttendanceNumberInput);
        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
    }
}
