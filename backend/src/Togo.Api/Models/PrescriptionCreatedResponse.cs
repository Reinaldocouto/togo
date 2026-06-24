namespace Togo.Api.Models;

public sealed record PrescriptionCreatedResponse(
    long Id,
    long AttendanceId,
    DateTime IssuedAt,
    int ItemCount);
