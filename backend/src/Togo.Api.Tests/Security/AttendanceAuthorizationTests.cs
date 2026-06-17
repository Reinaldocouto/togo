using System.Security.Claims;
using Togo.Api.Security;
using Togo.Application.Security;
using Togo.Domain.Security;

namespace Togo.Api.Tests.Security;

public sealed class AttendanceAuthorizationTests
{
    private static readonly string[] AttendancePermissionMatrix =
    [
        AttendancePermissions.Read,
        AttendancePermissions.Create,
        AttendancePermissions.Close,
        AttendancePermissions.Cancel
    ];

    [Theory]
    [InlineData(UserProfiles.Admin, AttendancePermissions.Read)]
    [InlineData(UserProfiles.Admin, AttendancePermissions.Create)]
    [InlineData(UserProfiles.Admin, AttendancePermissions.Close)]
    [InlineData(UserProfiles.Admin, AttendancePermissions.Cancel)]
    [InlineData(UserProfiles.Veterinarian, AttendancePermissions.Read)]
    [InlineData(UserProfiles.Veterinarian, AttendancePermissions.Create)]
    [InlineData(UserProfiles.Veterinarian, AttendancePermissions.Close)]
    [InlineData(UserProfiles.Veterinarian, AttendancePermissions.Cancel)]
    [InlineData(UserProfiles.Assistant, AttendancePermissions.Read)]
    [InlineData(UserProfiles.Assistant, AttendancePermissions.Create)]
    [InlineData(UserProfiles.Reception, AttendancePermissions.Read)]
    [InlineData(UserProfiles.Reception, AttendancePermissions.Create)]
    [InlineData(UserProfiles.Reception, AttendancePermissions.Cancel)]
    public void HasPermission_ShouldReturnTrue_WhenProfileHasAttendancePermission(string profile, string permission)
    {
        var user = CreateUser(profile);

        var hasPermission = AttendanceAuthorization.HasPermission(user, permission);

        Assert.True(hasPermission);
    }

    [Theory]
    [InlineData(UserProfiles.Assistant, AttendancePermissions.Close)]
    [InlineData(UserProfiles.Assistant, AttendancePermissions.Cancel)]
    [InlineData(UserProfiles.Reception, AttendancePermissions.Close)]
    [InlineData(UserProfiles.ReadOnly, AttendancePermissions.Read)]
    [InlineData(UserProfiles.ReadOnly, AttendancePermissions.Create)]
    [InlineData(UserProfiles.ReadOnly, AttendancePermissions.Close)]
    [InlineData(UserProfiles.ReadOnly, AttendancePermissions.Cancel)]
    public void HasPermission_ShouldReturnFalse_WhenProfileDoesNotHaveAttendancePermission(string profile, string permission)
    {
        var user = CreateUser(profile);

        var hasPermission = AttendanceAuthorization.HasPermission(user, permission);

        Assert.False(hasPermission);
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryAttendancePermission_WhenProfileClaimIsMissing()
    {
        var user = CreateUserWithoutProfile();

        Assert.All(AttendancePermissionMatrix, permission =>
            Assert.False(AttendanceAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryAttendancePermission_WhenProfileClaimIsEmpty()
    {
        var user = CreateUser(string.Empty);

        Assert.All(AttendancePermissionMatrix, permission =>
            Assert.False(AttendanceAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalseForEveryAttendancePermission_WhenProfileClaimIsInvalid()
    {
        var user = CreateUser("UnsupportedProfile");

        Assert.All(AttendancePermissionMatrix, permission =>
            Assert.False(AttendanceAuthorization.HasPermission(user, permission)));
    }

    [Fact]
    public void HasPermission_ShouldNormalizeProfileClaimCasing()
    {
        var user = CreateUser("veterinarian");

        Assert.True(AttendanceAuthorization.HasPermission(user, AttendancePermissions.Read));
        Assert.True(AttendanceAuthorization.HasPermission(user, AttendancePermissions.Create));
        Assert.True(AttendanceAuthorization.HasPermission(user, AttendancePermissions.Close));
        Assert.True(AttendanceAuthorization.HasPermission(user, AttendancePermissions.Cancel));
    }

    [Fact]
    public void HasPermission_ShouldReturnFalse_WhenPermissionIsUnknown()
    {
        var user = CreateUser(UserProfiles.Admin);

        var hasPermission = AttendanceAuthorization.HasPermission(user, "Attendance.Delete");

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
