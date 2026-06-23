namespace Togo.Application.Prescriptions.Contracts;

public sealed record PrescriptionListItemResponse(
    long Id,
    long AttendanceId,
    DateTime IssuedAt,
    int ItemCount);
