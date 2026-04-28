using Togo.Domain.Enums;

namespace Togo.Domain.Entities;

public class Attendance
{
    private Attendance() { }

    private Attendance(long patientId, string attendanceNumber, DateTime openedAt, DateTime? closedAt, AttendanceStatus status, AttendanceType type)
    {
        ValidateId(patientId, nameof(patientId));
        ValidateRequired(attendanceNumber, nameof(attendanceNumber));
        ValidateDate(openedAt, nameof(openedAt));
        if (closedAt.HasValue)
        {
            ValidateDate(closedAt.Value, nameof(closedAt));
        }

        PatientId = patientId;
        AttendanceNumber = attendanceNumber.Trim();
        OpenedAt = openedAt;
        ClosedAt = closedAt;
        Status = status;
        Type = type;
    }

    public long Id { get; private set; }
    public long PatientId { get; private set; }
    public string AttendanceNumber { get; private set; } = string.Empty;
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public AttendanceType Type { get; private set; }

    public static Attendance Create(long patientId, string attendanceNumber, DateTime openedAt, DateTime? closedAt, AttendanceStatus status, AttendanceType type) =>
        new(patientId, attendanceNumber, openedAt, closedAt, status, type);

    public void Close(DateTime closedAt)
    {
        ValidateDate(closedAt, nameof(closedAt));
        ClosedAt = closedAt;
        Status = AttendanceStatus.Closed;
    }

    private static void ValidateId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Id must be greater than zero");
        }
    }

    private static void ValidateRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required", paramName);
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
