namespace Togo.Domain.Entities;

public class UserClinicAccess
{
    private UserClinicAccess() { }

    private UserClinicAccess(Guid userId, long clinicId, DateTime createdAt)
    {
        ValidateUserId(userId, nameof(userId));
        ValidateClinicId(clinicId, nameof(clinicId));
        ValidateDate(createdAt, nameof(createdAt));

        UserId = userId;
        ClinicId = clinicId;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public long Id { get; private set; }
    public Guid UserId { get; private set; }
    public long ClinicId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static UserClinicAccess Create(Guid userId, long clinicId, DateTime createdAt) => new(userId, clinicId, createdAt);

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

    private static void ValidateUserId(Guid userId, string paramName)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required", paramName);
        }
    }

    private static void ValidateClinicId(long clinicId, string paramName)
    {
        if (clinicId <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "ClinicId must be greater than zero");
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
