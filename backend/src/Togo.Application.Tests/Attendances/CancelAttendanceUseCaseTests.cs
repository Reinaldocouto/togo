using Togo.Application.Attendances.UseCases;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Attendances;

public sealed class CancelAttendanceUseCaseTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenIdIsInvalid(long id)
    {
        var repository = new FakeAttendanceRepository();
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(id, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Attendance id is invalid.", result.Error);
        Assert.Null(result.Data);
        Assert.Equal(0, repository.GetByIdCallsCount);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenAttendanceDoesNotExist()
    {
        var repository = new FakeAttendanceRepository();
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(999, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Attendance not found.", result.Error);
        Assert.Null(result.Data);
        Assert.Equal(1, repository.GetByIdCallsCount);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenAttendanceIsOpen()
    {
        var repository = new FakeAttendanceRepository();
        const long lookupId = 223;
        var attendance = Attendance.Create(12, "ATT-CANCEL-001", new DateTime(2026, 03, 10, 9, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation);
        repository.AddAttendanceForLookup(lookupId, attendance);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(lookupId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(AttendanceStatus.Canceled, result.Data.Status);
        Assert.Null(result.Data.ClosedAt);
        Assert.Equal(1, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsClosed()
    {
        var repository = new FakeAttendanceRepository();
        const long lookupId = 224;
        var attendance = Attendance.Create(12, "ATT-CANCEL-002", new DateTime(2026, 03, 10, 9, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation);
        attendance.Close(new DateTime(2026, 03, 10, 10, 0, 0, DateTimeKind.Utc));
        repository.AddAttendanceForLookup(lookupId, attendance);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(lookupId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Closed attendance cannot be canceled", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsAlreadyCanceled()
    {
        var repository = new FakeAttendanceRepository();
        const long lookupId = 225;
        var attendance = Attendance.Create(12, "ATT-CANCEL-003", new DateTime(2026, 03, 10, 9, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation);
        attendance.Cancel();
        repository.AddAttendanceForLookup(lookupId, attendance);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(lookupId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Attendance is already canceled", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    private static CancelAttendanceUseCase CreateUseCase(FakeAttendanceRepository repository) =>
        new(repository, new TestLogger<CancelAttendanceUseCase>());
}
