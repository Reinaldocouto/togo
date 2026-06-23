using Togo.Application.Security;

namespace Togo.Application.Tests.Security;

public class PrescriptionPermissionsTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        Assert.Equal("Prescription.Read", PrescriptionPermissions.Read);
        Assert.Equal("Prescription.Create", PrescriptionPermissions.Create);
        Assert.Equal("Prescription.Update", PrescriptionPermissions.Update);
        Assert.Equal("Prescription.Cancel", PrescriptionPermissions.Cancel);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            PrescriptionPermissions.Read,
            PrescriptionPermissions.Create,
            PrescriptionPermissions.Update,
            PrescriptionPermissions.Cancel
        };

        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void Constants_ShouldNotDeclareDeletePermission()
    {
        var deleteField = typeof(PrescriptionPermissions).GetField("Delete");

        Assert.Null(deleteField);
    }
}
