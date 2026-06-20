using System.Security.Claims;
using Togo.Api.Security;
using Togo.Application.Security;
using Togo.Domain.Security;

namespace Togo.Api.Tests.Security;

public sealed class ClinicalEvolutionAuthorizationTests
{
    private static readonly string[] ClinicalEvolutionPermissionMatrix =
    [
        ClinicalEvolutionPermissions.Read,
        ClinicalEvolutionPermissions.Create,
        ClinicalEvolutionPermissions.Update
    ];

    [Theory]
    [InlineData(UserProfiles.Admin, ClinicalEvolutionPermissions.Read)]
    [InlineData(UserProfiles.Admin, ClinicalEvolutionPermissions.Create)]
    [InlineData(UserProfiles.Admin, ClinicalEvolutionPermissions.Update)]
    [InlineData(UserProfiles.Veterinarian, ClinicalEvolutionPermissions.Read)]
    [InlineData(UserProfiles.Veterinarian, ClinicalEvolutionPermissions.Create)]
    [InlineData(UserProfiles.Veterinarian, ClinicalEvolutionPermissions.Update)]
    [InlineData(UserProfiles.Assistant, ClinicalEvolutionPermissions.Read)]
    public void HasPermission_ShouldReturnTrue_WhenProfileHasClinicalEvolutionPermission(string profile, string permission)
    {
        var user = CreateUser(profile);

        var hasPermission = ClinicalEvolutionAuthorization.HasPermission(user, permission);

        Assert.True(hasPermission);
    }

    [Theory]
    [InlineData(UserProfiles.Assistant, ClinicalEvolutionPermissions.Create)]
    [InlineData(UserProfiles.Assistant, ClinicalEvolutionPermissions.Update)]
    [InlineData(UserProfiles.Reception, ClinicalEvolutionPermissions.Read)]
    [InlineData(UserProfiles.Reception, ClinicalEvolutionPermissions.Create)]
    [InlineData(UserProfiles.Reception, ClinicalEvolutionPermissions.Update)]
    [InlineData(UserProfiles.ReadOnly, ClinicalEvolutionPermissions.Read)]
    [InlineData(UserProfiles.ReadOnly, ClinicalEvolutionPermissions.Create)]
    [InlineData(UserProfiles.ReadOnly, ClinicalEvolutionPermissions.Update)]
    public void HasPermission_ShouldReturnFalse_WhenProfileDoesNotHaveClinicalEvolutionPermission(string profile, string permission)
    {
        var user = CreateUser(profile);

        var hasPermission = ClinicalEvolutionAuthorization.HasPermission(user, permission);

        Assert.False(hasPermission);
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryClinicalEvolutionPermission_WhenProfileClaimIsMissing()
    {
        var user = CreateUserWithoutProfile();

        Assert.All(ClinicalEvolutionPermissionMatrix, permission =>
            Assert.False(ClinicalEvolutionAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryClinicalEvolutionPermission_WhenProfileClaimIsEmpty()
    {
        var user = CreateUser(string.Empty);

        Assert.All(ClinicalEvolutionPermissionMatrix, permission =>
            Assert.False(ClinicalEvolutionAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryClinicalEvolutionPermission_WhenProfileClaimIsInvalid()
    {
        var user = CreateUser("UnsupportedProfile");

        Assert.All(ClinicalEvolutionPermissionMatrix, permission =>
            Assert.False(ClinicalEvolutionAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldNormalizeProfileClaimCasing()
    {
        var user = CreateUser("veterinarian");

        Assert.True(ClinicalEvolutionAuthorization.HasPermission(user, ClinicalEvolutionPermissions.Read));
        Assert.True(ClinicalEvolutionAuthorization.HasPermission(user, ClinicalEvolutionPermissions.Create));
        Assert.True(ClinicalEvolutionAuthorization.HasPermission(user, ClinicalEvolutionPermissions.Update));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalse_WhenPermissionIsUnknown()
    {
        var user = CreateUser(UserProfiles.Admin);

        var hasPermission = ClinicalEvolutionAuthorization.HasPermission(user, "ClinicalEvolution.Delete");

        Assert.False(hasPermission);
    }

    private static ClaimsPrincipal CreateUser(string profile) =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(TogoClaimTypes.Profile, profile)
        ], authenticationType: "Test"));

    private static ClaimsPrincipal CreateUserWithoutProfile() =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "test-user")
        ], authenticationType: "Test"));
}
