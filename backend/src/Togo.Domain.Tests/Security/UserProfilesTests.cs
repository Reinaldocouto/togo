using Togo.Domain.Security;
using Xunit;

namespace Togo.Domain.Tests.Security;

public class UserProfilesTests
{
    [Theory]
    [InlineData(UserProfiles.Admin)]
    [InlineData(UserProfiles.Veterinarian)]
    [InlineData(UserProfiles.Assistant)]
    [InlineData(UserProfiles.Reception)]
    [InlineData(UserProfiles.ReadOnly)]
    public void IsValid_ShouldReturnTrue_ForSupportedProfiles(string profile)
    {
        Assert.True(UserProfiles.IsValid(profile));
    }

    [Fact]
    public void Normalize_ShouldReturnCanonicalValue_WhenProfileHasDifferentCasingAndWhitespace()
    {
        Assert.Equal(UserProfiles.Reception, UserProfiles.Normalize("  reception  "));
    }
}
