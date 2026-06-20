using Togo.Api.Security;
using Togo.Application.Security;

namespace Togo.Api.Tests.Security;

public class ClinicalEvolutionPoliciesTests
{
    [Fact]
    public void Constants_ShouldMatchClinicalEvolutionPermissions()
    {
        Assert.Equal(ClinicalEvolutionPermissions.Read, ClinicalEvolutionPolicies.Read);
        Assert.Equal(ClinicalEvolutionPermissions.Create, ClinicalEvolutionPolicies.Create);
        Assert.Equal(ClinicalEvolutionPermissions.Update, ClinicalEvolutionPolicies.Update);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            ClinicalEvolutionPolicies.Read,
            ClinicalEvolutionPolicies.Create,
            ClinicalEvolutionPolicies.Update
        };

        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
