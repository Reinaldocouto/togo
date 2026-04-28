using Togo.Domain.Enums;

namespace Togo.Domain.Entities;

public class ClinicalEvolution
{
    private ClinicalEvolution() { }

    private ClinicalEvolution(long id, long attendanceId, DateTime registeredAt, EvolutionType type, string text)
    {
        ValidateId(id, nameof(id));
        ValidateId(attendanceId, nameof(attendanceId));
        ValidateDate(registeredAt, nameof(registeredAt));
        ValidateText(text);

        Id = id;
        AttendanceId = attendanceId;
        RegisteredAt = registeredAt;
        Type = type;
        Text = text.Trim();
    }

    public long Id { get; private set; }
    public long AttendanceId { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public EvolutionType Type { get; private set; }
    public string Text { get; private set; } = string.Empty;

    public static ClinicalEvolution Create(long id, long attendanceId, DateTime registeredAt, EvolutionType type, string text) =>
        new(id, attendanceId, registeredAt, type, text);

    public void UpdateText(string text)
    {
        ValidateText(text);
        Text = text.Trim();
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

    private static void ValidateDate(DateTime date, string paramName)
    {
        if (date == default)
        {
            throw new ArgumentException("Date is required", paramName);
        }
    }
}
