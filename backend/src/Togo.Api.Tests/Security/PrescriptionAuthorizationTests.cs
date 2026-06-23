using System.Security.Claims;
using Togo.Api.Security;
using Togo.Application.Security;
using Togo.Domain.Security;

namespace Togo.Api.Tests.Security;

public sealed class PrescriptionAuthorizationTests
{
    private static readonly string[] PrescriptionPermissionMatrix =
    [
        PrescriptionPermissions.Read,
        PrescriptionPermissions.Create,
        PrescriptionPermissions.Update,
        PrescriptionPermissions.Cancel
    ];

    [Theory]
    [InlineData(UserProfiles.Admin, PrescriptionPermissions.Read)]
    [InlineData(UserProfiles.Admin, PrescriptionPermissions.Create)]
    [InlineData(UserProfiles.Admin, PrescriptionPermissions.Update)]
    [InlineData(UserProfiles.Admin, PrescriptionPermissions.Cancel)]
    [InlineData(UserProfiles.Veterinarian, PrescriptionPermissions.Read)]
    [InlineData(UserProfiles.Veterinarian, PrescriptionPermissions.Create)]
    [InlineData(UserProfiles.Veterinarian, PrescriptionPermissions.Update)]
    [InlineData(UserProfiles.Veterinarian, PrescriptionPermissions.Cancel)]
    [InlineData(UserProfiles.Assistant, PrescriptionPermissions.Read)]
    public void HasPermission_ShouldReturnTrue_WhenProfileHasPrescriptionPermission(string profile, string permission)
    {
        var user = CreateUser(profile);

        var hasPermission = PrescriptionAuthorization.HasPermission(user, permission);

        Assert.True(hasPermission);
    }

    [Theory]
    [InlineData(UserProfiles.Assistant, PrescriptionPermissions.Create)]
    [InlineData(UserProfiles.Assistant, PrescriptionPermissions.Update)]
    [InlineData(UserProfiles.Assistant, PrescriptionPermissions.Cancel)]
    [InlineData(UserProfiles.Reception, PrescriptionPermissions.Read)]
    [InlineData(UserProfiles.Reception, PrescriptionPermissions.Create)]
    [InlineData(UserProfiles.Reception, PrescriptionPermissions.Update)]
    [InlineData(UserProfiles.Reception, PrescriptionPermissions.Cancel)]
    [InlineData(UserProfiles.ReadOnly, PrescriptionPermissions.Read)]
    [InlineData(UserProfiles.ReadOnly, PrescriptionPermissions.Create)]
    [InlineData(UserProfiles.ReadOnly, PrescriptionPermissions.Update)]
    [InlineData(UserProfiles.ReadOnly, PrescriptionPermissions.Cancel)]
    public void HasPermission_ShouldReturnFalse_WhenProfileDoesNotHavePrescriptionPermission(string profile, string permission)
    {
        var user = CreateUser(profile);

        var hasPermission = PrescriptionAuthorization.HasPermission(user, permission);

        Assert.False(hasPermission);
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryPrescriptionPermission_WhenProfileClaimIsMissing()
    {
        var user = CreateUserWithoutProfile();

        Assert.All(PrescriptionPermissionMatrix, permission =>
            Assert.False(PrescriptionAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryPrescriptionPermission_WhenProfileClaimIsEmpty()
    {
        var user = CreateUser(string.Empty);

        Assert.All(PrescriptionPermissionMatrix, permission =>
            Assert.False(PrescriptionAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryPrescriptionPermission_WhenProfileClaimIsInvalid()
    {
        var user = CreateUser("UnsupportedProfile");

        Assert.All(PrescriptionPermissionMatrix, permission =>
            Assert.False(PrescriptionAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldNormalizeProfileClaimCasing()
    {
        var user = CreateUser("veterinarian");

        Assert.True(PrescriptionAuthorization.HasPermission(user, PrescriptionPermissions.Read));
        Assert.True(PrescriptionAuthorization.HasPermission(user, PrescriptionPermissions.Create));
        Assert.True(PrescriptionAuthorization.HasPermission(user, PrescriptionPermissions.Update));
        Assert.True(PrescriptionAuthorization.HasPermission(user, PrescriptionPermissions.Cancel));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalse_WhenPermissionIsUnknown()
    {
        var user = CreateUser(UserProfiles.Admin);

        var hasPermission = PrescriptionAuthorization.HasPermission(user, "Prescription.Delete");

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
