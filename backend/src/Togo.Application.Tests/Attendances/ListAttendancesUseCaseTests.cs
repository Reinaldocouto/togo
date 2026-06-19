using Togo.Application.Attendances.UseCases;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Attendances;

public sealed class ListAttendancesUseCaseTests
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime TestCreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessWithEmptyList_WhenNoAttendancesExist()
    {
        var repository = new FakeAttendanceRepository();
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessWithAttendances_WhenAttendancesExist()
    {
        var repository = new FakeAttendanceRepository();

        var first = Attendance.Create(100, "ATT-LIST-001", new DateTime(2026, 03, 01, 8, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        var second = Attendance.Create(200, "ATT-LIST-002", new DateTime(2026, 03, 02, 9, 30, 0, DateTimeKind.Utc), AttendanceType.Emergency, TestUserId, TestCreatedAt);

        await repository.AddAsync(first, CancellationToken.None);
        await repository.AddAsync(second, CancellationToken.None);

        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);

        var firstResponse = result.Data.Single(x => x.AttendanceNumber == "ATT-LIST-001");
        Assert.Equal(100, firstResponse.PatientId);
        Assert.Equal("ATT-LIST-001", firstResponse.AttendanceNumber);
        Assert.Equal(AttendanceStatus.Open, firstResponse.Status);
        Assert.Equal(AttendanceType.Consultation, firstResponse.Type);

        var secondResponse = result.Data.Single(x => x.AttendanceNumber == "ATT-LIST-002");
        Assert.Equal(200, secondResponse.PatientId);
        Assert.Equal("ATT-LIST-002", secondResponse.AttendanceNumber);
        Assert.Equal(AttendanceStatus.Open, secondResponse.Status);
        Assert.Equal(AttendanceType.Emergency, secondResponse.Type);
    }

    private static ListAttendancesUseCase CreateUseCase(FakeAttendanceRepository repository) =>
        new(repository, new TestLogger<ListAttendancesUseCase>());
}
