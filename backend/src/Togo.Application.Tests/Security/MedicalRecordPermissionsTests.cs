using Togo.Application.Security;
using Xunit;

namespace Togo.Application.Tests.Security;

public class MedicalRecordPermissionsTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        Assert.Equal("MedicalRecord.Read", MedicalRecordPermissions.Read);
        Assert.Equal("MedicalRecord.Create", MedicalRecordPermissions.Create);
        Assert.Equal("MedicalRecord.Update", MedicalRecordPermissions.Update);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            MedicalRecordPermissions.Read,
            MedicalRecordPermissions.Create,
            MedicalRecordPermissions.Update
        };

        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}