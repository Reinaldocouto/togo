using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Togo.Application.Security;
using Togo.Domain.Security;

namespace Togo.Api.Security;

public static class PrescriptionAuthorization
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> PermissionsByProfile =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [UserProfiles.Admin] = CreatePermissionSet(
                PrescriptionPermissions.Read,
                PrescriptionPermissions.Create,
                PrescriptionPermissions.Update,
                PrescriptionPermissions.Cancel),
            [UserProfiles.Veterinarian] = CreatePermissionSet(
                PrescriptionPermissions.Read,
                PrescriptionPermissions.Create,
                PrescriptionPermissions.Update,
                PrescriptionPermissions.Cancel),
            [UserProfiles.Assistant] = CreatePermissionSet(PrescriptionPermissions.Read),
            [UserProfiles.Reception] = CreatePermissionSet(),
            [UserProfiles.ReadOnly] = CreatePermissionSet()
        };

    public static AuthorizationOptions AddPrescriptionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PrescriptionPolicies.Read, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, PrescriptionPermissions.Read)));

        options.AddPolicy(PrescriptionPolicies.Create, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, PrescriptionPermissions.Create)));

        options.AddPolicy(PrescriptionPolicies.Update, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, PrescriptionPermissions.Update)));

        options.AddPolicy(PrescriptionPolicies.Cancel, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, PrescriptionPermissions.Cancel)));

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
