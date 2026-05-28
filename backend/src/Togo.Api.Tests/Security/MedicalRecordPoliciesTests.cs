using Togo.Api.Security;
using Xunit;

namespace Togo.Api.Tests.Security;

public class MedicalRecordPoliciesTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        Assert.Equal("MedicalRecord.Read", MedicalRecordPolicies.Read);
        Assert.Equal("MedicalRecord.Create", MedicalRecordPolicies.Create);
        Assert.Equal("MedicalRecord.Update", MedicalRecordPolicies.Update);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            MedicalRecordPolicies.Read,
            MedicalRecordPolicies.Create,
            MedicalRecordPolicies.Update
        };

        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}