using Togo.Application.Auditing;

namespace Togo.Application.Tests.Auditing;

public class ClinicalEvolutionAuditActionsTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        Assert.Equal("ClinicalEvolution.Created", ClinicalEvolutionAuditActions.Created);
        Assert.Equal("ClinicalEvolution.Updated", ClinicalEvolutionAuditActions.Updated);
    }

    [Fact]
    public void Constants_ShouldBeNonEmpty_AndDistinct()
    {
        var values = new[]
        {
            ClinicalEvolutionAuditActions.Created,
            ClinicalEvolutionAuditActions.Updated
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
        var actionField = typeof(ClinicalEvolutionAuditActions).GetField(actionName);

        Assert.Null(actionField);
    }
}
