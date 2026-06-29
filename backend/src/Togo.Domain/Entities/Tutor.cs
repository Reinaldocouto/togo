namespace Togo.Domain.Entities;

public class Tutor
{
    private Tutor() { }

    private Tutor(long clinicId, string name, string? document, string? email, string? phone, DateTime createdAt)
    {
        ValidateClinicId(clinicId);
        ValidateName(name);
        ValidateDate(createdAt, nameof(createdAt));

        ClinicId = clinicId;
        Name = name.Trim();
        Document = NormalizeOptional(document);
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        CreatedAt = createdAt;
    }

    public long Id { get; private set; }
    public long ClinicId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Document { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static Tutor Create(long clinicId, string name, string? document, string? email, string? phone, DateTime createdAt) =>
        new(clinicId, name, document, email, phone, createdAt);

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

    private static void ValidateClinicId(long clinicId)
    {
        if (clinicId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(clinicId), "ClinicId must be greater than zero");
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required", nameof(name));
        }
    }

    private static void ValidateDate(DateTime dateCreateAt, string paramName)
    {
        if (dateCreateAt == default)
        {
            throw new ArgumentException("Date is required", paramName);
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
