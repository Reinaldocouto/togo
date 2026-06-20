using Togo.Application.Security;

namespace Togo.Api.Security;

public static class ClinicalEvolutionPolicies
{
    public const string Read = ClinicalEvolutionPermissions.Read;
    public const string Create = ClinicalEvolutionPermissions.Create;
    public const string Update = ClinicalEvolutionPermissions.Update;
}
