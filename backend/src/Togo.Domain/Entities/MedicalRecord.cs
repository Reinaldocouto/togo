using System.Text.Json;

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
        FlagsJson = NormalizeAndValidateFlagsJson(flagsJson, nameof(flagsJson));
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedByUserId = createdByUserId;
        UpdatedAt = createdAt;
        IsDeleted = false;
        DeletedAt = null;
        DeletedByUserId = null;
    }

    public long Id { get; private set; }
    public long PatientId { get; private set; }
    public string? GeneralNotes { get; private set; }
    public string? FlagsJson { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    public static MedicalRecord Create(long patientId, string? generalNotes, string? flagsJson, Guid createdByUserId, DateTime createdAt) =>
        new(patientId, generalNotes, flagsJson, createdByUserId, createdAt);

    public void UpdateNotes(string? generalNotes, string? flagsJson, Guid updatedByUserId, DateTime updatedAt)
    {
        ValidateUserId(updatedByUserId, nameof(updatedByUserId));
        ValidateDate(updatedAt, nameof(updatedAt));

        var normalizedGeneralNotes = NormalizeOptional(generalNotes);
        var normalizedFlagsJson = NormalizeAndValidateFlagsJson(flagsJson, nameof(flagsJson));

        GeneralNotes = normalizedGeneralNotes;
        FlagsJson = normalizedFlagsJson;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = updatedAt;
    }

    public void SoftDelete(Guid deletedByUserId, DateTime deletedAt)
    {
        ValidateUserId(deletedByUserId, nameof(deletedByUserId));
        ValidateDate(deletedAt, nameof(deletedAt));

        if (deletedAt.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Deleted at must be UTC", nameof(deletedAt));
        }

        if (IsDeleted)
        {
            throw new InvalidOperationException("Medical record is already soft deleted.");
        }

        IsDeleted = true;
        DeletedAt = deletedAt;
        DeletedByUserId = deletedByUserId;
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

    private static string? NormalizeAndValidateFlagsJson(string? flagsJson, string paramName)
    {
        if (string.IsNullOrWhiteSpace(flagsJson))
        {
            return null;
        }

        var normalizedFlagsJson = flagsJson.Trim();

        try
        {
            using var document = JsonDocument.Parse(normalizedFlagsJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("FlagsJson must be a valid JSON object.", paramName);
            }
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("FlagsJson must be a valid JSON object.", paramName, ex);
        }

        return normalizedFlagsJson;
    }
}
