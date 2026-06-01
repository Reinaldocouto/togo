using System.Security.Claims;
using Togo.Api.Security;
using Togo.Application.Security;
using Togo.Domain.Security;

namespace Togo.Api.Tests.Security;

public sealed class MedicalRecordAuthorizationTests
{
    private static readonly string[] MedicalRecordPermissionMatrix =
    [
        MedicalRecordPermissions.Read,
        MedicalRecordPermissions.Create,
        MedicalRecordPermissions.Update
    ];

    [Theory]
    [InlineData(UserProfiles.Admin, MedicalRecordPermissions.Read)]
    [InlineData(UserProfiles.Admin, MedicalRecordPermissions.Create)]
    [InlineData(UserProfiles.Admin, MedicalRecordPermissions.Update)]
    [InlineData(UserProfiles.Veterinarian, MedicalRecordPermissions.Read)]
    [InlineData(UserProfiles.Veterinarian, MedicalRecordPermissions.Create)]
    [InlineData(UserProfiles.Veterinarian, MedicalRecordPermissions.Update)]
    [InlineData(UserProfiles.Assistant, MedicalRecordPermissions.Read)]
    public void HasPermission_ShouldReturnTrue_WhenProfileHasMedicalRecordPermission(string profile, string permission)
    {
        var user = CreateUser(profile);

        var hasPermission = MedicalRecordAuthorization.HasPermission(user, permission);

        Assert.True(hasPermission);
    }

    [Theory]
    [InlineData(UserProfiles.Assistant, MedicalRecordPermissions.Create)]
    [InlineData(UserProfiles.Assistant, MedicalRecordPermissions.Update)]
    [InlineData(UserProfiles.Reception, MedicalRecordPermissions.Read)]
    [InlineData(UserProfiles.Reception, MedicalRecordPermissions.Create)]
    [InlineData(UserProfiles.Reception, MedicalRecordPermissions.Update)]
    [InlineData(UserProfiles.ReadOnly, MedicalRecordPermissions.Read)]
    [InlineData(UserProfiles.ReadOnly, MedicalRecordPermissions.Create)]
    [InlineData(UserProfiles.ReadOnly, MedicalRecordPermissions.Update)]
    public void HasPermission_ShouldReturnFalse_WhenProfileDoesNotHaveMedicalRecordPermission(string profile, string permission)
    {
        var user = CreateUser(profile);

        var hasPermission = MedicalRecordAuthorization.HasPermission(user, permission);

        Assert.False(hasPermission);
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryMedicalRecordPermission_WhenProfileClaimIsMissing()
    {
        var user = CreateUserWithoutProfile();

        Assert.All(MedicalRecordPermissionMatrix, permission =>
            Assert.False(MedicalRecordAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryMedicalRecordPermission_WhenProfileClaimIsEmpty()
    {
        var user = CreateUser(string.Empty);

        Assert.All(MedicalRecordPermissionMatrix, permission =>
            Assert.False(MedicalRecordAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryMedicalRecordPermission_WhenProfileClaimIsInvalid()
    {
        var user = CreateUser("UnsupportedProfile");

        Assert.All(MedicalRecordPermissionMatrix, permission =>
            Assert.False(MedicalRecordAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldNormalizeProfileClaimCasing()
    {
        var user = CreateUser("veterinarian");

        Assert.True(MedicalRecordAuthorization.HasPermission(user, MedicalRecordPermissions.Read));
        Assert.True(MedicalRecordAuthorization.HasPermission(user, MedicalRecordPermissions.Create));
        Assert.True(MedicalRecordAuthorization.HasPermission(user, MedicalRecordPermissions.Update));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalse_WhenPermissionIsUnknown()
    {
        var user = CreateUser(UserProfiles.Admin);

        var hasPermission = MedicalRecordAuthorization.HasPermission(user, "MedicalRecord.Delete");

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
