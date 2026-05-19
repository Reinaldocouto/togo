using Togo.Domain.Enums;

namespace Togo.Application.Attendances.Contracts;

public record CreateAttendanceRequest(
    long PatientId,
    string AttendanceNumber,
    DateTime OpenedAt,
    AttendanceType Type);
