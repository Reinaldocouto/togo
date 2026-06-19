using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Attendances.Fakes;

public sealed class FakeAttendanceRepositoryTests
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime TestCreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    [Fact]
    public async Task AddAsync_ShouldNotOverwriteAttendances_WhenEntitiesHaveDefaultId()
    {
        var repository = new FakeAttendanceRepository();
        var attendanceOne = Attendance.Create(1, "ATT-001", new DateTime(2026, 1, 1), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        var attendanceTwo = Attendance.Create(2, "ATT-002", new DateTime(2026, 1, 2), AttendanceType.Emergency, TestUserId, TestCreatedAt);

        await repository.AddAsync(attendanceOne, CancellationToken.None);
        await repository.AddAsync(attendanceTwo, CancellationToken.None);

        var list = await repository.ListAsync(CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.Contains(attendanceOne, list);
        Assert.Contains(attendanceTwo, list);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnConfiguredLookupAttendance()
    {
        var repository = new FakeAttendanceRepository();
        var attendance = Attendance.Create(3, "ATT-LOOKUP", new DateTime(2026, 1, 3), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        repository.AddAttendanceForLookup(999, attendance);

        var result = await repository.GetByIdAsync(999, CancellationToken.None);

        Assert.Same(attendance, result);
        Assert.Equal(1, repository.GetByIdCallsCount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldIncrementUpdateCallsCount_AndKeepMultipleItems()
    {
        var repository = new FakeAttendanceRepository();
        var attendanceOne = Attendance.Create(1, "ATT-001", new DateTime(2026, 1, 1), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        var attendanceTwo = Attendance.Create(2, "ATT-002", new DateTime(2026, 1, 2), AttendanceType.Emergency, TestUserId, TestCreatedAt);

        await repository.AddAsync(attendanceOne, CancellationToken.None);
        await repository.AddAsync(attendanceTwo, CancellationToken.None);
        await repository.UpdateAsync(attendanceOne, CancellationToken.None);

        var list = await repository.ListAsync(CancellationToken.None);

        Assert.Equal(1, repository.UpdateCallsCount);
        Assert.Equal(2, list.Count);
        Assert.Contains(attendanceOne, list);
        Assert.Contains(attendanceTwo, list);
    }
}
