namespace Togo.Domain.Entities;

public class Prescription
{
    private Prescription() { }

    private Prescription(long attendanceId, DateTime issuedAt, string? notes)
    {
        ValidateId(attendanceId, nameof(attendanceId));
        ValidateDate(issuedAt, nameof(issuedAt));

        AttendanceId = attendanceId;
        IssuedAt = issuedAt;
        Notes = NormalizeOptional(notes);
    }

    public long Id { get; private set; }
    public long AttendanceId { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public string? Notes { get; private set; }

    public static Prescription Create(long attendanceId, DateTime issuedAt, string? notes) =>
        new(attendanceId, issuedAt, notes);

    public void UpdateNotes(string? notes)
    {
        Notes = NormalizeOptional(notes);
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
