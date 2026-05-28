using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Togo.Domain.Entities;
using Togo.Domain.Security;
using Togo.Infrastructure.Tokens;
using Xunit;

namespace Togo.Infrastructure.Tests.Tokens;

public class JwtTokenServiceTests
{
    [Fact]
    public void IssueToken_ShouldEmitProfileClaimAndExistingUserClaims()
    {
        // Arrange
        var user = User.Create("Jane Doe", "jane@example.com", "hashed-password", UserProfiles.Veterinarian);
        var service = CreateService();

        // Act
        var token = service.IssueToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        Assert.Contains(jwt.Claims, claim => claim.Type == TogoClaimTypes.Profile && claim.Value == UserProfiles.Veterinarian);
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == user.Email);
    }

    [Fact]
    public void TryValidateToken_ShouldReturnUserId_WhenTokenIsValid()
    {
        // Arrange
        var user = User.Create("Jane Doe", "jane@example.com", "hashed-password", UserProfiles.Assistant);
        var service = CreateService();
        var token = service.IssueToken(user);

        // Act
        var isValid = service.TryValidateToken(token, out var userId);

        // Assert
        Assert.True(isValid);
        Assert.Equal(user.Id, userId);
    }

    [Fact]
    public void TryValidateToken_ShouldReturnFalse_WhenTokenIsEmpty()
    {
        // Arrange
        var service = CreateService();

        // Act
        var isValid = service.TryValidateToken("   ", out var userId);

        // Assert
        Assert.False(isValid);
        Assert.Equal(Guid.Empty, userId);
    }

    private static JwtTokenService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "Togo.Tests",
                ["Jwt:Audience"] = "Togo.Tests.Client",
                ["Jwt:Secret"] = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                ["Jwt:ExpirationMinutes"] = "120"
            })
            .Build();

        return new JwtTokenService(configuration);
    }
}
