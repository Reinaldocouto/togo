using Togo.Api.Security;
using Togo.Application.Security;

namespace Togo.Api.Tests.Security;

public class PrescriptionPoliciesTests
{
    [Fact]
    public void Constants_ShouldMatchPrescriptionPermissions()
    {
        Assert.Equal(PrescriptionPermissions.Read, PrescriptionPolicies.Read);
        Assert.Equal(PrescriptionPermissions.Create, PrescriptionPolicies.Create);
        Assert.Equal(PrescriptionPermissions.Update, PrescriptionPolicies.Update);
        Assert.Equal(PrescriptionPermissions.Cancel, PrescriptionPolicies.Cancel);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            PrescriptionPolicies.Read,
            PrescriptionPolicies.Create,
            PrescriptionPolicies.Update,
            PrescriptionPolicies.Cancel
        };

        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
