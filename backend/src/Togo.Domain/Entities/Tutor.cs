namespace Togo.Domain.Entities;

public class Tutor
{
    private Tutor() { }

    private Tutor(long id, string name, string? document, string? email, string? phone, DateTime createdAt)
    {
        ValidateId(id, nameof(id));
        ValidateName(name);
        ValidateDate(createdAt, nameof(createdAt));

        Id = id;
        Name = name.Trim();
        Document = NormalizeOptional(document);
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        CreatedAt = createdAt;
    }

    public long Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Document { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static Tutor Create(long id, string name, string? document, string? email, string? phone, DateTime createdAt) =>
        new(id, name, document, email, phone, createdAt);

    public void UpdateContact(string? document, string? email, string? phone, DateTime updatedAt)
    {
        ValidateDate(updatedAt, nameof(updatedAt));

        Document = NormalizeOptional(document);
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        UpdatedAt = updatedAt;
    }

    public void UpdateName(string name, DateTime updatedAt)
    {
        ValidateName(name);
        ValidateDate(updatedAt, nameof(updatedAt));

        Name = name.Trim();
        UpdatedAt = updatedAt;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required", nameof(name));
        }
    }

    private static void ValidateId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Id must be greater than zero");
        }
    }

    private static void ValidateDate(DateTime date, string paramName)
    {
        if (date == default)
        {
            throw new ArgumentException("Date is required", paramName);
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
