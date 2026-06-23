namespace Togo.Application.Prescriptions.Contracts;

public sealed record CreatePrescriptionRequest(
    long AttendanceId,
    DateTime IssuedAt,
    string? Notes,
    IReadOnlyList<CreatePrescriptionItemRequest> Items);
