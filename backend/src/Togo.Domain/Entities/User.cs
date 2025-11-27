using System.Text.RegularExpressions;

namespace Togo.Domain.Entities;

public class User
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private User(Guid id, string name, string email, string passwordHash)
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public static User Create(string name, string email, string passwordHash)
    {
        ValidateName(name);
        ValidateEmail(email);
        ValidatePasswordHash(passwordHash);

        return new User(Guid.NewGuid(), name.Trim(), NormalizeEmail(email), passwordHash);
    }

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name.Trim();
    }

    public void UpdateEmail(string email)
    {
        ValidateEmail(email);
        Email = NormalizeEmail(email);
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        ValidatePasswordHash(passwordHash);
        PasswordHash = passwordHash;
    }

    public static void EnsurePasswordMeetsRules(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty", nameof(password));
        }

        if (password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long", nameof(password));
        }
    }

    public static bool IsEmailValid(string email) => !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty", nameof(name));
        }
    }

    private static void ValidateEmail(string email)
    {
        if (!IsEmailValid(email))
        {
            throw new ArgumentException("Email is not valid", nameof(email));
        }
    }

    private static void ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
