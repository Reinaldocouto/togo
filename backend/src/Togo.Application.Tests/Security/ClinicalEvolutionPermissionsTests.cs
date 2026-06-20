using Togo.Application.Security;

namespace Togo.Application.Tests.Security;

public class ClinicalEvolutionPermissionsTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        Assert.Equal("ClinicalEvolution.Read", ClinicalEvolutionPermissions.Read);
        Assert.Equal("ClinicalEvolution.Create", ClinicalEvolutionPermissions.Create);
        Assert.Equal("ClinicalEvolution.Update", ClinicalEvolutionPermissions.Update);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            ClinicalEvolutionPermissions.Read,
            ClinicalEvolutionPermissions.Create,
            ClinicalEvolutionPermissions.Update
        };

        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void Constants_ShouldNotDeclareDeletePermission()
    {
        var deleteField = typeof(ClinicalEvolutionPermissions).GetField("Delete");

        Assert.Null(deleteField);
    }
}
