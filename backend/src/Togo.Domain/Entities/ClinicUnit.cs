namespace Togo.Domain.Entities;

public class ClinicUnit
{
    private ClinicUnit() { }

    private ClinicUnit(long clinicId, string name, DateTime createdAt)
    {
        ValidateId(clinicId, nameof(clinicId));
        ValidateName(name);
        ValidateDate(createdAt, nameof(createdAt));

        ClinicId = clinicId;
        Name = name.Trim();
        IsActive = true;
        CreatedAt = createdAt;
    }

    public long Id { get; private set; }
    public long ClinicId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static ClinicUnit Create(long clinicId, string name, DateTime createdAt) => new(clinicId, name, createdAt);

    public void Activate(DateTime updatedAt)
    {
        ValidateDate(updatedAt, nameof(updatedAt));

        IsActive = true;
        UpdatedAt = updatedAt;
    }

    public void Inactivate(DateTime updatedAt)
    {
        ValidateDate(updatedAt, nameof(updatedAt));

        IsActive = false;
        UpdatedAt = updatedAt;
    }

    private static void ValidateId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Id must be greater than zero");
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required", nameof(name));
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
