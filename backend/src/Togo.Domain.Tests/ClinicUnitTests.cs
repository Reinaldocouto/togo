using Togo.Domain.Entities;

namespace Togo.Domain.Tests;

public class ClinicUnitTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActiveClinicUnitLinkedToClinic()
    {
        var createdAt = DateTime.UtcNow;

        var unit = ClinicUnit.Create(1, "Unidade Norte", createdAt);

        Assert.Equal(1, unit.ClinicId);
        Assert.Equal("Unidade Norte", unit.Name);
        Assert.True(unit.IsActive);
        Assert.Equal(createdAt, unit.CreatedAt);
        Assert.Null(unit.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_InvalidClinicId_ShouldThrowArgumentOutOfRangeException(long clinicId)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => ClinicUnit.Create(clinicId, "Unidade Norte", DateTime.UtcNow));

        Assert.StartsWith("Id must be greater than zero", exception.Message);
        Assert.Equal("clinicId", exception.ParamName);
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => ClinicUnit.Create(1, "", DateTime.UtcNow));

        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Create_WhitespaceName_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => ClinicUnit.Create(1, "   ", DateTime.UtcNow));

        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Inactivate_ValidData_ShouldInactivateClinicUnit()
    {
        var unit = ClinicUnit.Create(1, "Unidade Norte", DateTime.UtcNow);
        var updatedAt = DateTime.UtcNow.AddMinutes(1);

        unit.Inactivate(updatedAt);

        Assert.False(unit.IsActive);
        Assert.Equal(updatedAt, unit.UpdatedAt);
    }
}
