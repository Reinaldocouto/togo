using Togo.Domain.Enums;

namespace Togo.Application.Attendances.Contracts;

public record AttendanceResponse(
    long Id,
    long PatientId,
    string AttendanceNumber,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    AttendanceStatus Status,
    AttendanceType Type);
