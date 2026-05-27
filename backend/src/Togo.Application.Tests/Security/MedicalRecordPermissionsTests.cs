using FluentAssertions;
using Togo.Application.Security;
using Xunit;

namespace Togo.Application.Tests.Security;

public class MedicalRecordPermissionsTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        MedicalRecordPermissions.Read.Should().Be("MedicalRecord.Read");
        MedicalRecordPermissions.Create.Should().Be("MedicalRecord.Create");
        MedicalRecordPermissions.Update.Should().Be("MedicalRecord.Update");
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

        values.Should().OnlyContain(value => !string.IsNullOrWhiteSpace(value));
        values.Distinct().Should().HaveCount(values.Length);
    }
}
