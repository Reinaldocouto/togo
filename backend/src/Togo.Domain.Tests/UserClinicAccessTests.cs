using Togo.Domain.Entities;

namespace Togo.Domain.Tests;

public class UserClinicAccessTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActiveAccess()
    {
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var access = UserClinicAccess.Create(userId, 10, createdAt);

        Assert.Equal(userId, access.UserId);
        Assert.Equal(10, access.ClinicId);
        Assert.True(access.IsActive);
        Assert.Equal(createdAt, access.CreatedAt);
        Assert.Null(access.UpdatedAt);
    }

    [Fact]
    public void Create_EmptyUserId_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => UserClinicAccess.Create(Guid.Empty, 10, DateTime.UtcNow));

        Assert.StartsWith("UserId is required", exception.Message);
        Assert.Equal("userId", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_InvalidClinicId_ShouldThrowArgumentOutOfRangeException(long clinicId)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => UserClinicAccess.Create(Guid.NewGuid(), clinicId, DateTime.UtcNow));

        Assert.StartsWith("ClinicId must be greater than zero", exception.Message);
        Assert.Equal("clinicId", exception.ParamName);
    }

    [Fact]
    public void Inactivate_ValidData_ShouldInactivateAccess()
    {
        var access = UserClinicAccess.Create(Guid.NewGuid(), 10, DateTime.UtcNow);
        var updatedAt = DateTime.UtcNow.AddMinutes(1);

        access.Inactivate(updatedAt);

        Assert.False(access.IsActive);
        Assert.Equal(updatedAt, access.UpdatedAt);
    }

    [Fact]
    public void Activate_ValidData_ShouldReactivateAccess()
    {
        var access = UserClinicAccess.Create(Guid.NewGuid(), 10, DateTime.UtcNow);
        var inactivatedAt = DateTime.UtcNow.AddMinutes(1);
        var reactivatedAt = DateTime.UtcNow.AddMinutes(2);
        access.Inactivate(inactivatedAt);

        access.Activate(reactivatedAt);

        Assert.True(access.IsActive);
        Assert.Equal(reactivatedAt, access.UpdatedAt);
    }
}
