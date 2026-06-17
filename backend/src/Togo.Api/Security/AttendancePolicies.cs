using Togo.Application.Security;

namespace Togo.Api.Security;

public static class AttendancePolicies
{
    public const string Read = AttendancePermissions.Read;
    public const string Create = AttendancePermissions.Create;
    public const string Close = AttendancePermissions.Close;
    public const string Cancel = AttendancePermissions.Cancel;
}
