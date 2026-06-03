namespace Togo.Domain.Entities;

public class MedicalRecord
{
    private MedicalRecord() { }

    private MedicalRecord(long patientId, string? generalNotes, string? flagsJson, Guid createdByUserId, DateTime createdAt)
    {
        ValidateId(patientId, nameof(patientId));
        ValidateUserId(createdByUserId, nameof(createdByUserId));
        ValidateDate(createdAt, nameof(createdAt));

        PatientId = patientId;
        GeneralNotes = NormalizeOptional(generalNotes);
        FlagsJson = NormalizeOptional(flagsJson);
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedByUserId = createdByUserId;
        UpdatedAt = createdAt;
    }

    public long Id { get; private set; }
    public long PatientId { get; private set; }
    public string? GeneralNotes { get; private set; }
    public string? FlagsJson { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static MedicalRecord Create(long patientId, string? generalNotes, string? flagsJson, Guid createdByUserId, DateTime createdAt) =>
        new(patientId, generalNotes, flagsJson, createdByUserId, createdAt);

    public void UpdateNotes(string? generalNotes, string? flagsJson, Guid updatedByUserId, DateTime updatedAt)
    {
        ValidateUserId(updatedByUserId, nameof(updatedByUserId));
        ValidateDate(updatedAt, nameof(updatedAt));

        GeneralNotes = NormalizeOptional(generalNotes);
        FlagsJson = NormalizeOptional(flagsJson);
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = updatedAt;
    }

    private static void ValidateId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Id must be greater than zero");
        }
    }

    private static void ValidateUserId(Guid userId, string paramName)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required", paramName);
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
