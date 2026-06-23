using Togo.Application.Auditing;

namespace Togo.Application.Tests.Auditing;

public class PrescriptionAuditActionsTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        Assert.Equal("Prescription.Created", PrescriptionAuditActions.Created);
        Assert.Equal("Prescription.Updated", PrescriptionAuditActions.Updated);
        Assert.Equal("Prescription.Canceled", PrescriptionAuditActions.Canceled);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            PrescriptionAuditActions.Created,
            PrescriptionAuditActions.Updated,
            PrescriptionAuditActions.Canceled
        };

        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Theory]
    [InlineData("Deleted")]
    [InlineData("Read")]
    [InlineData("AccessDenied")]
    public void Constants_ShouldNotDeclareOutOfScopeActions(string actionName)
    {
        var actionField = typeof(PrescriptionAuditActions).GetField(actionName);

        Assert.Null(actionField);
    }
}
