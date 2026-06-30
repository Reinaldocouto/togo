using Togo.Application.Attendances.UseCases;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Attendances;

public sealed class GetAttendanceByIdUseCaseTests
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime TestCreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
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
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenAttendanceExists()
    {
        var repository = new FakeAttendanceRepository();
        const long attendanceLookupId = 123;
        var openedAt = new DateTime(2026, 02, 10, 09, 00, 00, DateTimeKind.Utc);
        var attendance = Attendance.Create(1, 12, "ATT-GET-001", openedAt, AttendanceType.Consultation, TestUserId, TestCreatedAt);
        repository.AddAttendanceForLookup(attendanceLookupId, attendance);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(attendanceLookupId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(12, result.Data.PatientId);
        Assert.Equal("ATT-GET-001", result.Data.AttendanceNumber);
        Assert.Equal(openedAt, result.Data.OpenedAt);
        Assert.Null(result.Data.ClosedAt);
        Assert.Equal(AttendanceStatus.Open, result.Data.Status);
        Assert.Equal(AttendanceType.Consultation, result.Data.Type);
        Assert.Equal(1, repository.GetByIdCallsCount);
    }

    private static GetAttendanceByIdUseCase CreateUseCase(FakeAttendanceRepository repository) =>
        new(repository, new TestLogger<GetAttendanceByIdUseCase>());
}
