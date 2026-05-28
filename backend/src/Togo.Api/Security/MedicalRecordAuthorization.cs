using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Togo.Application.Security;
using Togo.Domain.Security;

namespace Togo.Api.Security;

public static class MedicalRecordAuthorization
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> PermissionsByProfile =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [UserProfiles.Admin] = CreatePermissionSet(
                MedicalRecordPermissions.Read,
                MedicalRecordPermissions.Create,
                MedicalRecordPermissions.Update),
            [UserProfiles.Veterinarian] = CreatePermissionSet(
                MedicalRecordPermissions.Read,
                MedicalRecordPermissions.Create,
                MedicalRecordPermissions.Update),
            [UserProfiles.Assistant] = CreatePermissionSet(MedicalRecordPermissions.Read),
            [UserProfiles.Reception] = CreatePermissionSet(),
            [UserProfiles.ReadOnly] = CreatePermissionSet()
        };

    public static AuthorizationOptions AddMedicalRecordPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(MedicalRecordPolicies.Read, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, MedicalRecordPermissions.Read)));

        options.AddPolicy(MedicalRecordPolicies.Create, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, MedicalRecordPermissions.Create)));

        options.AddPolicy(MedicalRecordPolicies.Update, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, MedicalRecordPermissions.Update)));

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
