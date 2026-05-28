namespace Togo.Domain.Security;

public static class UserProfiles
{
    public const string Admin = "Admin";
    public const string Veterinarian = "Veterinarian";
    public const string Assistant = "Assistant";
    public const string Reception = "Reception";
    public const string ReadOnly = "ReadOnly";

    public const string Default = ReadOnly;

    private static readonly string[] Values =
    [
        Admin,
        Veterinarian,
        Assistant,
        Reception,
        ReadOnly
    ];

    public static bool IsValid(string? profile) =>
        !string.IsNullOrWhiteSpace(profile)
        && Values.Any(value => string.Equals(value, profile.Trim(), StringComparison.OrdinalIgnoreCase));

    public static string Normalize(string? profile)
    {
        if (string.IsNullOrWhiteSpace(profile))
        {
            throw new ArgumentException("Profile cannot be empty", nameof(profile));
        }

        var normalized = Values.FirstOrDefault(value =>
            string.Equals(value, profile.Trim(), StringComparison.OrdinalIgnoreCase));

        if (normalized is null)
        {
            throw new ArgumentException("Profile is not supported", nameof(profile));
        }

        return normalized;
    }
}
