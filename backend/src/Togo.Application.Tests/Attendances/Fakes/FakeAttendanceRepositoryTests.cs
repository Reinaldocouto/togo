using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Attendances.Fakes;

public sealed class FakeAttendanceRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldNotOverwriteAttendances_WhenEntitiesHaveDefaultId()
    {
        var repository = new FakeAttendanceRepository();
        var attendanceOne = Attendance.Create(1, "ATT-001", new DateTime(2026, 1, 1), AttendanceType.Consultation);
        var attendanceTwo = Attendance.Create(2, "ATT-002", new DateTime(2026, 1, 2), AttendanceType.Emergency);

        await repository.AddAsync(attendanceOne, CancellationToken.None);
        await repository.AddAsync(attendanceTwo, CancellationToken.None);

        var list = await repository.ListAsync(CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.Contains(attendanceOne, list);
        Assert.Contains(attendanceTwo, list);
    }
}
