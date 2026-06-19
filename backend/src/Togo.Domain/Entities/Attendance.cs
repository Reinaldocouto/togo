using Togo.Domain.Enums;

namespace Togo.Domain.Entities;

public class Attendance
{
    private Attendance() { }

    private Attendance(long patientId, string attendanceNumber, DateTime openedAt, AttendanceType type, Guid createdByUserId, DateTime createdAtUtc)
    {
        ValidateId(patientId, nameof(patientId));
        ValidateRequired(attendanceNumber, nameof(attendanceNumber));
        ValidateDate(openedAt, nameof(openedAt));
        ValidateUserId(createdByUserId, nameof(createdByUserId));
        ValidateDate(createdAtUtc, nameof(createdAtUtc));

        PatientId = patientId;
        AttendanceNumber = attendanceNumber.Trim();
        OpenedAt = openedAt;
        ClosedAt = null;
        Status = AttendanceStatus.Open;
        Type = type;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAtUtc;
        UpdatedByUserId = createdByUserId;
        UpdatedAt = createdAtUtc;
    }

    public long Id { get; private set; }
    public long PatientId { get; private set; }
    public string AttendanceNumber { get; private set; } = string.Empty;
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public AttendanceType Type { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? ClosedByUserId { get; private set; }
    public Guid? CanceledByUserId { get; private set; }
    public DateTime? CanceledAt { get; private set; }

    public static Attendance Create(long patientId, string attendanceNumber, DateTime openedAt, AttendanceType type, Guid createdByUserId, DateTime createdAtUtc) =>
        new(patientId, attendanceNumber, openedAt, type, createdByUserId, createdAtUtc);

    public void Close(DateTime closedAt, Guid closedByUserId, DateTime updatedAtUtc)
    {
        if (Status == AttendanceStatus.Canceled)
        {
            throw new InvalidOperationException("Canceled attendance cannot be closed");
        }

        if (Status == AttendanceStatus.Closed)
        {
            throw new InvalidOperationException("Attendance is already closed");
        }

        ValidateDate(closedAt, nameof(closedAt));
        ValidateUserId(closedByUserId, nameof(closedByUserId));
        ValidateDate(updatedAtUtc, nameof(updatedAtUtc));

        if (closedAt < OpenedAt)
        {
            throw new ArgumentException("ClosedAt cannot be before OpenedAt", nameof(closedAt));
        }

        ClosedAt = closedAt;
        ClosedByUserId = closedByUserId;
        UpdatedByUserId = closedByUserId;
        UpdatedAt = updatedAtUtc;
        Status = AttendanceStatus.Closed;
    }

    public void Cancel(Guid canceledByUserId, DateTime canceledAtUtc)
    {
        if (Status == AttendanceStatus.Closed)
        {
            throw new InvalidOperationException("Closed attendance cannot be canceled");
        }

        if (Status == AttendanceStatus.Canceled)
        {
            throw new InvalidOperationException("Attendance is already canceled");
        }

        ValidateUserId(canceledByUserId, nameof(canceledByUserId));
        ValidateDate(canceledAtUtc, nameof(canceledAtUtc));

        Status = AttendanceStatus.Canceled;
        ClosedAt = null;
        CanceledByUserId = canceledByUserId;
        CanceledAt = canceledAtUtc;
        UpdatedByUserId = canceledByUserId;
        UpdatedAt = canceledAtUtc;
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
