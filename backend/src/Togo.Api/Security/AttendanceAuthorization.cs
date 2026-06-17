using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Togo.Application.Security;
using Togo.Domain.Security;

namespace Togo.Api.Security;

public static class AttendanceAuthorization
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> PermissionsByProfile =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [UserProfiles.Admin] = CreatePermissionSet(
                AttendancePermissions.Read,
                AttendancePermissions.Create,
                AttendancePermissions.Close,
                AttendancePermissions.Cancel),
            [UserProfiles.Veterinarian] = CreatePermissionSet(
                AttendancePermissions.Read,
                AttendancePermissions.Create,
                AttendancePermissions.Close,
                AttendancePermissions.Cancel),
            [UserProfiles.Assistant] = CreatePermissionSet(
                AttendancePermissions.Read,
                AttendancePermissions.Create),
            [UserProfiles.Reception] = CreatePermissionSet(
                AttendancePermissions.Read,
                AttendancePermissions.Create,
                AttendancePermissions.Cancel),
            [UserProfiles.ReadOnly] = CreatePermissionSet()
        };

    public static AuthorizationOptions AddAttendancePolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(AttendancePolicies.Read, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, AttendancePermissions.Read)));

        options.AddPolicy(AttendancePolicies.Create, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, AttendancePermissions.Create)));

        options.AddPolicy(AttendancePolicies.Close, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, AttendancePermissions.Close)));

        options.AddPolicy(AttendancePolicies.Cancel, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, AttendancePermissions.Cancel)));

        return options;
    }

    public static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        var profile = user.FindFirstValue(TogoClaimTypes.Profile);
        if (!UserProfiles.IsValid(profile))
        {
            return false;
        }

        var normalizedProfile = UserProfiles.Normalize(profile);
        return PermissionsByProfile.TryGetValue(normalizedProfile, out var permissions)
            && permissions.Contains(permission);
    }

    private static IReadOnlySet<string> CreatePermissionSet(params string[] permissions) =>
        new HashSet<string>(permissions, StringComparer.Ordinal);
}
