using Togo.Domain.Entities;

namespace Togo.Domain.Tests;

public class ClinicTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActiveClinicLinkedToOrganization()
    {
        var createdAt = DateTime.UtcNow;

        var clinic = Clinic.Create(1, "Clínica Centro", createdAt);

        Assert.Equal(1, clinic.OrganizationId);
        Assert.Equal("Clínica Centro", clinic.Name);
        Assert.True(clinic.IsActive);
        Assert.Equal(createdAt, clinic.CreatedAt);
        Assert.Null(clinic.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_InvalidOrganizationId_ShouldThrowArgumentOutOfRangeException(long organizationId)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Clinic.Create(organizationId, "Clínica Centro", DateTime.UtcNow));

        Assert.StartsWith("Id must be greater than zero", exception.Message);
        Assert.Equal("organizationId", exception.ParamName);
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Clinic.Create(1, "", DateTime.UtcNow));

        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Create_WhitespaceName_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Clinic.Create(1, "   ", DateTime.UtcNow));

        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Inactivate_ValidData_ShouldInactivateClinic()
    {
        var clinic = Clinic.Create(1, "Clínica Centro", DateTime.UtcNow);
        var updatedAt = DateTime.UtcNow.AddMinutes(1);

        clinic.Inactivate(updatedAt);

        Assert.False(clinic.IsActive);
        Assert.Equal(updatedAt, clinic.UpdatedAt);
    }
}
