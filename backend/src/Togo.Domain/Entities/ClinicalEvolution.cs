using Togo.Domain.Enums;

namespace Togo.Domain.Entities;

public class ClinicalEvolution
{
    private ClinicalEvolution() { }

    private ClinicalEvolution(long attendanceId, DateTime registeredAt, EvolutionType type, string text, Guid createdByUserId, DateTime createdAt)
    {
        ValidateId(attendanceId, nameof(attendanceId));
        ValidateDate(registeredAt, nameof(registeredAt));
        ValidateText(text);
        ValidateUserId(createdByUserId, nameof(createdByUserId));
        ValidateDate(createdAt, nameof(createdAt));

        AttendanceId = attendanceId;
        RegisteredAt = registeredAt;
        Type = type;
        Text = text.Trim();
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedByUserId = createdByUserId;
        UpdatedAt = createdAt;
    }

    public long Id { get; private set; }
    public long AttendanceId { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public EvolutionType Type { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static ClinicalEvolution Create(long attendanceId, DateTime registeredAt, EvolutionType type, string text, Guid createdByUserId, DateTime createdAt) =>
        new(attendanceId, registeredAt, type, text, createdByUserId, createdAt);

    public void UpdateText(string text, Guid updatedByUserId, DateTime updatedAt)
    {
        ValidateText(text);
        ValidateUserId(updatedByUserId, nameof(updatedByUserId));
        ValidateDate(updatedAt, nameof(updatedAt));

        Text = text.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = updatedAt;
    }

    private static void ValidateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text is required", nameof(text));
        }
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
}
