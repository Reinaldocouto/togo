using Togo.Application.Security;

namespace Togo.Application.Tests.Security;

public class AttendancePermissionsTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        Assert.Equal("Attendance.Read", AttendancePermissions.Read);
        Assert.Equal("Attendance.Create", AttendancePermissions.Create);
        Assert.Equal("Attendance.Close", AttendancePermissions.Close);
        Assert.Equal("Attendance.Cancel", AttendancePermissions.Cancel);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            AttendancePermissions.Read,
            AttendancePermissions.Create,
            AttendancePermissions.Close,
            AttendancePermissions.Cancel
        };

        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
