namespace Togo.Application.Prescriptions.Contracts;

public sealed record PrescriptionResponse(
    long Id,
    long AttendanceId,
    DateTime IssuedAt,
    string? Notes,
    IReadOnlyList<PrescriptionItemResponse> Items);
