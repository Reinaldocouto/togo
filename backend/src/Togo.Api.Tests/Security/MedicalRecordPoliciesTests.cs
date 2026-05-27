using FluentAssertions;
using Togo.Api.Security;
using Xunit;

namespace Togo.Api.Tests.Security;

public class MedicalRecordPoliciesTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        MedicalRecordPolicies.Read.Should().Be("MedicalRecord.Read");
        MedicalRecordPolicies.Create.Should().Be("MedicalRecord.Create");
        MedicalRecordPolicies.Update.Should().Be("MedicalRecord.Update");
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

        values.Should().OnlyContain(value => !string.IsNullOrWhiteSpace(value));
        values.Distinct().Should().HaveCount(values.Length);
    }
}
