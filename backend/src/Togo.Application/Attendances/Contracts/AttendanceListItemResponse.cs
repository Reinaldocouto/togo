using Togo.Domain.Enums;

namespace Togo.Application.Attendances.Contracts;

public record AttendanceListItemResponse(
    long Id,
    long PatientId,
    string AttendanceNumber,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    AttendanceStatus Status,
    AttendanceType Type);
