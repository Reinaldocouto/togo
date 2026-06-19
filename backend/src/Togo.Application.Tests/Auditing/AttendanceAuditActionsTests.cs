using Togo.Application.Auditing;

namespace Togo.Application.Tests.Auditing;

public class AttendanceAuditActionsTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        Assert.Equal("Attendance.Created", AttendanceAuditActions.Created);
        Assert.Equal("Attendance.Closed", AttendanceAuditActions.Closed);
        Assert.Equal("Attendance.Canceled", AttendanceAuditActions.Canceled);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            AttendanceAuditActions.Created,
            AttendanceAuditActions.Closed,
            AttendanceAuditActions.Canceled
        };

        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
