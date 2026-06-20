using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Togo.Application.Security;
using Togo.Domain.Security;

namespace Togo.Api.Security;

public static class ClinicalEvolutionAuthorization
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> PermissionsByProfile =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [UserProfiles.Admin] = CreatePermissionSet(
                ClinicalEvolutionPermissions.Read,
                ClinicalEvolutionPermissions.Create,
                ClinicalEvolutionPermissions.Update),
            [UserProfiles.Veterinarian] = CreatePermissionSet(
                ClinicalEvolutionPermissions.Read,
                ClinicalEvolutionPermissions.Create,
                ClinicalEvolutionPermissions.Update),
            [UserProfiles.Assistant] = CreatePermissionSet(ClinicalEvolutionPermissions.Read),
            [UserProfiles.Reception] = CreatePermissionSet(),
            [UserProfiles.ReadOnly] = CreatePermissionSet()
        };

    public static AuthorizationOptions AddClinicalEvolutionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(ClinicalEvolutionPolicies.Read, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, ClinicalEvolutionPermissions.Read)));

        options.AddPolicy(ClinicalEvolutionPolicies.Create, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, ClinicalEvolutionPermissions.Create)));

        options.AddPolicy(ClinicalEvolutionPolicies.Update, policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(context => HasPermission(context.User, ClinicalEvolutionPermissions.Update)));

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
