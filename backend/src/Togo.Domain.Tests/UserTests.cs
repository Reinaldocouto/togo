using Togo.Domain.Entities;
using Togo.Domain.Security;
using Xunit;

namespace Togo.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_ShouldCreateUserWithProfile_WhenProfileIsValid()
    {
        // Act
        var user = User.Create("  Jane Doe  ", "  JANE@example.com  ", "hashed-password", UserProfiles.Veterinarian);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Jane Doe", user.Name);
        Assert.Equal("jane@example.com", user.Email);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal(UserProfiles.Veterinarian, user.Profile);
    }

    [Fact]
    public void Create_ShouldUseReadOnlyProfile_WhenProfileIsNotProvided()
    {
        // Act
        var user = User.Create("Jane Doe", "jane@example.com", "hashed-password");

        // Assert
        Assert.Equal(UserProfiles.ReadOnly, user.Profile);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenProfileIsEmpty(string? profile)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            User.Create("Jane Doe", "jane@example.com", "hashed-password", profile));
        Assert.StartsWith("Profile cannot be empty", exception.Message);
        Assert.Equal("profile", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenProfileIsUnsupported()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            User.Create("Jane Doe", "jane@example.com", "hashed-password", "Manager"));
        Assert.StartsWith("Profile is not supported", exception.Message);
        Assert.Equal("profile", exception.ParamName);
    }

    [Fact]
    public void UpdateProfile_ShouldNormalizeProfile_WhenProfileIsValid()
    {
        // Arrange
        var user = User.Create("Jane Doe", "jane@example.com", "hashed-password");

        // Act
        user.UpdateProfile("  assistant  ");

        // Assert
        Assert.Equal(UserProfiles.Assistant, user.Profile);
    }
}
