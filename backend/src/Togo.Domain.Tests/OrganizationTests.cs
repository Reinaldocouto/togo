using Togo.Domain.Entities;

namespace Togo.Domain.Tests;

public class OrganizationTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActiveOrganization()
    {
        var createdAt = DateTime.UtcNow;

        var organization = Organization.Create("TOGO Saúde Animal", createdAt);

        Assert.Equal("TOGO Saúde Animal", organization.Name);
        Assert.True(organization.IsActive);
        Assert.Equal(createdAt, organization.CreatedAt);
        Assert.Null(organization.UpdatedAt);
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Organization.Create("", DateTime.UtcNow));

        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Create_WhitespaceName_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Organization.Create("   ", DateTime.UtcNow));

        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Inactivate_ValidData_ShouldInactivateOrganization()
    {
        var organization = Organization.Create("TOGO Saúde Animal", DateTime.UtcNow);
        var updatedAt = DateTime.UtcNow.AddMinutes(1);

        organization.Inactivate(updatedAt);

        Assert.False(organization.IsActive);
        Assert.Equal(updatedAt, organization.UpdatedAt);
    }
}
