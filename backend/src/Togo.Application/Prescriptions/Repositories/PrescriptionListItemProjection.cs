namespace Togo.Application.Prescriptions.Repositories;

public sealed record PrescriptionListItemProjection(
    long Id,
    long AttendanceId,
    DateTime IssuedAt,
    int ItemCount);
