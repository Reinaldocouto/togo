using Togo.Api.Security;
using Togo.Application.Security;

namespace Togo.Api.Tests.Security;

public class AttendancePoliciesTests
{
    [Fact]
    public void Constants_ShouldMatchAttendancePermissions()
    {
        Assert.Equal(AttendancePermissions.Read, AttendancePolicies.Read);
        Assert.Equal(AttendancePermissions.Create, AttendancePolicies.Create);
        Assert.Equal(AttendancePermissions.Close, AttendancePolicies.Close);
        Assert.Equal(AttendancePermissions.Cancel, AttendancePolicies.Cancel);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            AttendancePolicies.Read,
            AttendancePolicies.Create,
            AttendancePolicies.Close,
            AttendancePolicies.Cancel
        };

        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
