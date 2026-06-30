namespace Togo.Application.Prescriptions.Contracts;

public sealed record PrescriptionResponse(
    long Id,
    long ClinicId,
    long AttendanceId,
    DateTime IssuedAt,
    string? Notes,
    IReadOnlyList<PrescriptionItemResponse> Items);
