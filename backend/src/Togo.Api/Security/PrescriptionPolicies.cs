using Togo.Application.Security;

namespace Togo.Api.Security;

public static class PrescriptionPolicies
{
    public const string Read = PrescriptionPermissions.Read;
    public const string Create = PrescriptionPermissions.Create;
    public const string Update = PrescriptionPermissions.Update;
    public const string Cancel = PrescriptionPermissions.Cancel;
}
