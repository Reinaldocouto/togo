using Togo.Domain.Enums;

namespace Togo.Domain.Entities;

public class Patient
{
    private Patient() { }

    private Patient(long clinicId, PatientType type, string name, DateOnly? birthDate, string status, DateTime createdAt)
    {
        ValidateClinicId(clinicId);
        ValidateName(name);
        ValidateStatus(status);
        ValidateDate(createdAt, nameof(createdAt));

        ClinicId = clinicId;
        Type = type;
        Name = name.Trim();
        BirthDate = birthDate;
        Status = status.Trim();
        CreatedAt = createdAt;
    }

    public long Id { get; private set; }
    public long ClinicId { get; private set; }
    public PatientType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateOnly? BirthDate { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static Patient Create(long clinicId, PatientType type, string name, DateOnly? birthDate, string status, DateTime createdAt) =>
        new(clinicId, type, name, birthDate, status, createdAt);

    public void Update(string name, DateOnly? birthDate, string status, DateTime updatedAt)
    {
        ValidateName(name);
        ValidateStatus(status);
        ValidateDate(updatedAt, nameof(updatedAt));

        Name = name.Trim();
        BirthDate = birthDate;
        Status = status.Trim();
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

    private static void ValidateStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status is required", nameof(status));
        }
    }

    private static void ValidateDate(DateTime date, string paramName)
    {
        if (date == default)
        {
            throw new ArgumentException("Date is required", paramName);
        }
    }
}
