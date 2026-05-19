using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.UseCases;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Attendances;

public sealed class CloseAttendanceUseCaseTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenIdIsInvalid(long id)
    {
        var repository = new FakeAttendanceRepository();
        var useCase = CreateUseCase(repository);
        var request = new CloseAttendanceRequest(new DateTime(2026, 02, 11, 9, 0, 0, DateTimeKind.Utc));

        var result = await useCase.ExecuteAsync(id, request, CancellationToken.None);

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
        var request = new CloseAttendanceRequest(new DateTime(2026, 02, 11, 9, 0, 0, DateTimeKind.Utc));

        var result = await useCase.ExecuteAsync(999, request, CancellationToken.None);

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
        const long lookupId = 123;
        var openedAt = new DateTime(2026, 02, 10, 9, 0, 0, DateTimeKind.Utc);
        var closedAt = new DateTime(2026, 02, 10, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(12, "ATT-CLOSE-001", openedAt, AttendanceType.Consultation);
        repository.AddAttendanceForLookup(lookupId, attendance);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(lookupId, new CloseAttendanceRequest(closedAt), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(AttendanceStatus.Closed, result.Data.Status);
        Assert.Equal(closedAt, result.Data.ClosedAt);
        Assert.Equal(1, repository.GetByIdCallsCount);
        Assert.Equal(1, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenClosedAtIsDefault()
    {
        var repository = new FakeAttendanceRepository();
        const long lookupId = 124;
        var openedAt = new DateTime(2026, 02, 10, 9, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(12, "ATT-CLOSE-002", openedAt, AttendanceType.Consultation);
        repository.AddAttendanceForLookup(lookupId, attendance);
        var useCase = CreateUseCase(repository);
        var closedAt = default(DateTime);

        var result = await useCase.ExecuteAsync(lookupId, new CloseAttendanceRequest(closedAt), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.StartsWith("Date is required", result.Error);
        Assert.Equal(1, repository.GetByIdCallsCount);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenClosedAtIsBeforeOpenedAt()
    {
        var repository = new FakeAttendanceRepository();
        const long lookupId = 125;
        var openedAt = new DateTime(2026, 02, 10, 9, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(12, "ATT-CLOSE-003", openedAt, AttendanceType.Consultation);
        repository.AddAttendanceForLookup(lookupId, attendance);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(
            lookupId,
            new CloseAttendanceRequest(new DateTime(2026, 02, 10, 8, 59, 59, DateTimeKind.Utc)),
            CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Close date cannot be earlier than open date.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsAlreadyClosed()
    {
        var repository = new FakeAttendanceRepository();
        const long lookupId = 126;
        var openedAt = new DateTime(2026, 02, 10, 9, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(12, "ATT-CLOSE-004", openedAt, AttendanceType.Consultation);
        attendance.Close(new DateTime(2026, 02, 10, 10, 0, 0, DateTimeKind.Utc));
        repository.AddAttendanceForLookup(lookupId, attendance);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(
            lookupId,
            new CloseAttendanceRequest(new DateTime(2026, 02, 10, 11, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Only open attendance can be closed.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsCanceled()
    {
        var repository = new FakeAttendanceRepository();
        const long lookupId = 127;
        var openedAt = new DateTime(2026, 02, 10, 9, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(12, "ATT-CLOSE-005", openedAt, AttendanceType.Consultation);
        attendance.Cancel();
        repository.AddAttendanceForLookup(lookupId, attendance);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(
            lookupId,
            new CloseAttendanceRequest(new DateTime(2026, 02, 10, 11, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Only open attendance can be closed.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    private static CloseAttendanceUseCase CreateUseCase(FakeAttendanceRepository repository) =>
        new(repository, new TestLogger<CloseAttendanceUseCase>());
}
